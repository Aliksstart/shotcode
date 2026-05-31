using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace Core
{
    public enum ValutState { 
        Locked,
        Unlocked,
        Dirty
    }
    public abstract class Valut
    {
        private readonly List<Block> _blocks;
        private readonly SCDB _db;
        private Action _lockedEvent;

        private ValutState _state = ValutState.Locked;
        private readonly System.Timers.Timer _autoLockTimer; //System.Timers.Timer(_ => OnAutoLockEvent(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan)

        public bool IsDirty => _state == ValutState.Dirty;

        protected void MarkDirty()
        {
            if (_state == ValutState.Unlocked || _state == ValutState.Dirty)
                _state = ValutState.Dirty;
            else
                throw new InvalidOperationException("Can't modify locked vault.");
        }

        public Valut(SCDB db, int interval, Action lockedEvent)
        {
            _blocks = new List<Block>();
            _db = db;
            _autoLockTimer = new System.Timers.Timer(interval);
            _autoLockTimer.Elapsed += _autoLockTimer_Elapsed;
            _lockedEvent = lockedEvent;
        }

        private void _autoLockTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Close();
        }

        private void ParseValut(byte[] decrypted_text) 
        {
            if (decrypted_text.Length < 4)
                throw new InvalidDataException("Invalid database. origin payload corrupted");
            uint count_blocks = BinaryPrimitives.ReadUInt32LittleEndian(
                decrypted_text.AsSpan(0, 4));
            int start_block = 4;
            if (count_blocks > 0) {
                for (int i = 0; i < count_blocks; i++)
                {
                    if (start_block + BlockLayout.MinBlockSize > decrypted_text.Length)
                        throw new InvalidDataException("Corruption database: minimal block size exceeds decrypted payload.");
                    byte version = decrypted_text[start_block + BlockLayout.VersionOffset];
                    if (version != Block.CurrentVersion)
                    {
                        throw new NotSupportedException("Unsupported version block");
                    }
                    byte type = decrypted_text[start_block + BlockLayout.TypeOffset];
                    byte digits = decrypted_text[start_block + BlockLayout.DigitsOffset];
                    byte algorithm = decrypted_text[start_block + BlockLayout.AlgorithmOffset];
                    ulong ts_created = BinaryPrimitives.ReadUInt64LittleEndian(
                        decrypted_text.AsSpan(
                            start_block + BlockLayout.CreatedTSOffset, BlockLayout.TimestampSize));
                    ulong ts_updated = BinaryPrimitives.ReadUInt64LittleEndian(
                        decrypted_text.AsSpan(
                            start_block + BlockLayout.UpdatedTSOffset, BlockLayout.TimestampSize));
                    ulong period_or_counter = BinaryPrimitives.ReadUInt64LittleEndian(
                        decrypted_text.AsSpan(
                             start_block + BlockLayout.PeriodOrCounterOffset, BlockLayout.PeriodOrCounterSize));
                    uint u_len_service_name = BinaryPrimitives.ReadUInt32LittleEndian(
                        decrypted_text.AsSpan(
                            start_block + BlockLayout.ServiceNameLengthOffset, BlockLayout.LengthSize));
                    uint u_len_secret = BinaryPrimitives.ReadUInt32LittleEndian(
                        decrypted_text.AsSpan(
                            start_block + BlockLayout.SecretLengthOffset, BlockLayout.LengthSize));
                    uint u_len_extra = BinaryPrimitives.ReadUInt32LittleEndian(
                        decrypted_text.AsSpan(
                            start_block + BlockLayout.ExtraLengthOffset, BlockLayout.LengthSize));
                    int offset = start_block + BlockLayout.DataOffset;
                    if (u_len_extra != 0)
                        throw new InvalidDataException("Extra unsupported");

                    if (u_len_service_name == 0)
                        throw new InvalidDataException("Corruption databse len service name is 0");

                    int len_service_name = checked((int)u_len_service_name);
                    int len_secret = checked((int)u_len_secret);
                    int len_extra = checked((int)u_len_extra);

                    if (offset + len_service_name > decrypted_text.Length)
                        throw new InvalidDataException("Corruption database length service_name + header block over decrypted text");
                    byte[] byte_service_name = new byte[len_service_name];
                    decrypted_text.AsSpan(offset, len_service_name).CopyTo(byte_service_name);
                    string service_name = Encoding.UTF8.GetString(byte_service_name);
                    offset += len_service_name;
                    if (len_secret == 0)
                        throw new InvalidDataException("Corruption databse len secret is 0");
                    if(offset + len_secret > decrypted_text.Length)
                        throw new InvalidDataException("Corruption database length secret + service_name + header block over decrypted text");
                    byte[] secrets = new byte[len_secret];
                    decrypted_text.AsSpan(offset, len_secret).CopyTo(secrets);
                    offset += len_secret;
                    if (offset + len_extra > decrypted_text.Length)
                        throw new InvalidDataException("Corruption database length extra + secret + service_name + header block over decrypted text");
                    byte[] extra = Array.Empty<byte>();

                    Block block = new Block(type, digits, algorithm, ts_created, ts_updated, period_or_counter, service_name, secrets, extra); 
                    _blocks.Add(block);
                    start_block = offset;
                }
            }
        }

        protected virtual void Open(byte[] key, byte[] nonce, byte[] tag, byte[] ciphertext)
        {
            byte[] decrypted_text = new byte[ciphertext.Length];
            using (AesGcm aesGcm = new AesGcm(key, tag.Length))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, decrypted_text);
            }
            try
            {
                ParseValut(decrypted_text);
                _state = ValutState.Unlocked;
                _autoLockTimer.Start();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(decrypted_text);
            }
        }

        protected void SerializePayload(MemoryStream ms)
        {
            // перевести на memoryStream
            Span<byte> buf_count = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf_count, checked((uint)_blocks.Count));
            ms.Write(buf_count);
            foreach (Block block in _blocks) 
                block.WriteTo(ms);
        }

        public abstract void Save();
        protected void AddBlock(Block block)
        {
            _blocks.Add(block);
            MarkDirty();
        }
        protected bool RemoveBlock(Block block)
        {
            bool removed = _blocks.Remove(block);
            if (removed) {
                MarkDirty();
            }
            return removed;
        }

        protected virtual void Close() {
            _autoLockTimer.Stop();
            _state = ValutState.Locked;
            foreach (Block block in _blocks)
            {
                block.SecreatClear();
            }
            _blocks.Clear();
            _lockedEvent();
        }
    }
}
