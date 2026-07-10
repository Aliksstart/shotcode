using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace Core
{
    public enum VaultState { 
        Locked,
        Unlocked,
        Dirty
    }
    public abstract class Vault : IDisposable
    {
        protected readonly object _lock;
        private readonly List<Block> _blocks;
        protected readonly SCDB _db;
        private Action _lockedEvent;

        private VaultState _state = VaultState.Locked;
        private readonly System.Timers.Timer _autoLockTimer;

        public bool IsDirty => _state == VaultState.Dirty;

        protected void MarkDirty()
        {
            if (_state == VaultState.Unlocked || _state == VaultState.Dirty)
                _state = VaultState.Dirty;
            else
                throw new InvalidOperationException("Can't modify locked vault.");
        }

        public Vault(SCDB db, int interval, Action lockedEvent, VaultState vs)
        {
            _lock = new object();
            _blocks = new List<Block>();
            _db = db;
            _autoLockTimer = new System.Timers.Timer(interval);
            _autoLockTimer.Elapsed += _autoLockTimer_Elapsed;
            _lockedEvent = lockedEvent;
            _state = vs;
        }

        private void _autoLockTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Close();
        }

        private void ParseVault(byte[] decrypted_text) 
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

        protected virtual void Open(Span<byte> key, byte[] nonce, byte[] tag, byte[] ciphertext)
        {
            byte[] decrypted_text = new byte[ciphertext.Length];
            using (AesGcm aesGcm = new AesGcm(key, tag.Length))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, decrypted_text);
            }
            try
            {
                ParseVault(decrypted_text);
                _state = VaultState.Unlocked;
                _autoLockTimer.Start();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(decrypted_text);
            }
        }

        protected void EncSerializePayload(MemoryStream ms, Span<byte> key, out byte[] nonce, out byte[] gcm_tag)
        {
            if (_state == VaultState.Locked)
                throw new InvalidOperationException("Can't save locked vault.");
            nonce = new byte[SCDBLayout.NonceSize];
            RandomNumberGenerator.Fill(nonce);
            gcm_tag = new byte[SCDBLayout.GcmTagSize];
            using MemoryStream local = new MemoryStream();
            Span<byte> buf_count = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf_count, checked((uint)_blocks.Count));
            local.Write(buf_count);
            foreach (Block block in _blocks) 
                block.WriteTo(local);
            ReadOnlySpan<byte> plaintext = local.GetBuffer().AsSpan(0, (int)local.Length);
            byte[] ciphertext = new byte[plaintext.Length];

            using (AesGcm aes = new AesGcm(key, gcm_tag.Length))
            {
                aes.Encrypt(nonce, plaintext, ciphertext, gcm_tag);
            }
            CryptographicOperations.ZeroMemory(local.GetBuffer().AsSpan(0, (int)local.Length));
            ms.Write(ciphertext);
        }

        public virtual void Save()
        {
            if (_state == VaultState.Locked)
            {
                throw new InvalidOperationException("Can't work with a locked vault.");
            }
            else
            {
                _state = VaultState.Unlocked;
                if (!_autoLockTimer.Enabled)
                {
                    _autoLockTimer.Start();
                }
            }
        }
        protected string[] Services 
        {
            get {
                lock (_lock)
                {
                    if (_state != VaultState.Locked)
                    {
                        string[] services = new string[_blocks.Count];
                        for (int i = 0; i < _blocks.Count; i++)
                        {
                            services[i] = _blocks[i].NameService;
                        }
                        return services;
                    }
                    else
                    {
                        throw new InvalidOperationException("Can't work with a locked vault.");
                    }
                }
            }
        }
        protected bool AddBlock(Block block)
        {
            lock (_lock)
            {
                if (_state == VaultState.Locked)
                    throw new InvalidOperationException("Can't work with a locked vault.");
                foreach (var e in _blocks)
                {
                    if (e.NameService == block.NameService)
                        return false;
                }
                _blocks.Add(block);
                MarkDirty();
                return true;
            }
        }
        private bool RemoveBlock(Block block)
        {
            bool removed = _blocks.Remove(block);
            if (removed) {
                MarkDirty();
            }
            return removed;
        }

        protected bool RemoveBlock(string name)
        {
            lock (_lock)
            {
                if (_state == VaultState.Locked)
                    throw new InvalidOperationException("Can't work with a locked vault.");
                foreach (var e in _blocks)
                {
                    if (e.NameService == name)
                    {
                        return RemoveBlock(e);
                    }
                }
                return false;
            }
        }

        public int GetCode(string name)
        {
            lock (_lock)
            {
                if (_state == VaultState.Locked)
                   throw new InvalidOperationException("Can't work with a locked vault.");
                foreach (var e in _blocks)
                {
                    if (e.NameService == name)
                    {
                        return e.Code;
                    }
                }
                return -1;
            }
        }
        public String GetCodeString(string name)
        {
            lock (_lock)
            { 
                if (_state == VaultState.Locked)
                    throw new InvalidOperationException("Can't work with a locked vault.");
                foreach (var e in _blocks)
                    if (e.NameService == name)
                        return e.CodeString;
                return String.Empty;
            }
        }

        protected virtual void Close() {
            bool transitioned = false;
            lock (_lock)
            {
                if (_state != VaultState.Locked)
                {
                    _autoLockTimer.Dispose();
                    OnLocking();
                    foreach (Block block in _blocks)
                        block.SecretClear();
                    _blocks.Clear();
                    _state = VaultState.Locked;
                    transitioned = true;
                }
            }

            if (transitioned)
                _lockedEvent();
        }

        protected abstract void OnLocking();

        public virtual void Dispose() => Close();
    }
}
