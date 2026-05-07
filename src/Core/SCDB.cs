using System.Buffers.Binary;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core
{
    public class SCDB : IDisposable
    {
        private static ReadOnlySpan<byte> MagicConst => "SCDB"u8;
        public const uint CurrentVersion = 1;

        private byte[] _magic = new byte[SCDBLayout.MagicSize];
        private uint _version;
        private ulong _created_ts;
        private ulong _updated_ts;
        private ulong _tpm_updated_ts; // not used before version 2.0 and later
        private byte[] _nonce_origin = new byte[SCDBLayout.NonceSize];
        private byte[] _nonce_tpm = new byte[SCDBLayout.NonceSize]; // not used before version 2.0 and later
        private byte[] _reserved = new byte[SCDBLayout.ReservedSize];
        private uint _len_crypto_origin;
        private uint _len_crypto_tpm;
        private byte[] _origin_gcm_tag = new byte[SCDBLayout.GcmTagSize];
        private byte[] _tpm_gcm_tag = new byte[SCDBLayout.GcmTagSize]; // not used before version 2.0 and later
        private byte[] _crypted_origin = Array.Empty<byte>();
        private byte[] _crypted_tpm = Array.Empty<byte>(); // not used before version 2.0 and later

        private Stream stream;

        private static DateTime convertTS(ulong ts ) => DateTimeOffset.FromUnixTimeMilliseconds((long)ts).DateTime;
        public DateTime Created {
            get { return convertTS(_created_ts); }
        }
        public DateTime Updated
        {
            get { return convertTS(_updated_ts); }
        }


        private bool IsUnusedField(ReadOnlySpan<byte> field) {
            foreach (byte b in field)
            {
                if (b != 0xFF)
                    return false;
            }
            return true;
        }

        private void createBaseStruct() {
            MagicConst.CopyTo(_magic);
            _version = CurrentVersion;
            _created_ts = _updated_ts = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            _tpm_updated_ts = ulong.MaxValue;
            Array.Clear(_nonce_origin);
            Array.Fill(_nonce_tpm, (byte)0xFF);
            Array.Fill(_reserved, (byte)0xFF);
            _len_crypto_origin = 0;
            _len_crypto_tpm = 0;
            Array.Clear(_origin_gcm_tag);
            Array.Fill(_tpm_gcm_tag, (byte)0xFF);
        }
        private uint ReadUInt32()
        {
            Span<byte> buf = stackalloc byte[4];
            stream.ReadExactly(buf);
            return BinaryPrimitives.ReadUInt32LittleEndian(buf);
        }

        private ulong ReadUInt64()
        {
            Span<byte> buf = stackalloc byte[8];
            stream.ReadExactly(buf);
            return BinaryPrimitives.ReadUInt64LittleEndian(buf);
        }
        private void ReaderAllFile() {
            if (!stream.CanRead)
                throw new IOException("Stream is not readtebel");
            if (stream.Length < SCDBLayout.MinFileSize)
                throw new InvalidDataException("File too short to be a valid SCDB.");
            stream.Position = 0;
            stream.ReadExactly(_magic);
            if (!_magic.AsSpan().SequenceEqual(MagicConst))
                throw new InvalidDataException("Format file unknown");
            _version = ReadUInt32();
            if (_version != 1)
                throw new InvalidDataException("Unsupported version");
            _created_ts = ReadUInt64();
            _updated_ts = ReadUInt64();
            _tpm_updated_ts = ReadUInt64();
            stream.ReadExactly(_nonce_origin);
            stream.ReadExactly(_nonce_tpm);
            stream.ReadExactly(_reserved);
            _len_crypto_origin = ReadUInt32();
            _len_crypto_tpm = ReadUInt32();
            stream.ReadExactly(_origin_gcm_tag);
            stream.ReadExactly(_tpm_gcm_tag);
            if (_len_crypto_origin != 0)
            {
                _crypted_origin = new byte[_len_crypto_origin];
                stream.ReadExactly(_crypted_origin);
            }
            
        }
        private void writeUint32(uint value) 
        {
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf, value);
            stream.Write(buf);
        }
        private void writeUint64(ulong value)
        {
            Span<byte> buf = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(buf, value);
            stream.Write(buf);
        }
        private void WriterAllFile() {
            if (!stream.CanWrite)
                throw new IOException("Stream is not writabel");
            if (_version != 1)
                throw new NotSupportedException("Unsupported version: "+_version.ToString());
            if (!IsUnusedField(_reserved))
                throw new InvalidDataException("Used reversed bytes");
            if (_len_crypto_tpm != 0 || _crypted_tpm.Length != 0 || !IsUnusedField(_tpm_gcm_tag) || !IsUnusedField(_nonce_tpm) || _tpm_updated_ts != ulong.MaxValue)
                throw new InvalidDataException("Fill unused tpm value");

            if (_len_crypto_origin != _crypted_origin.Length)
                throw new InvalidDataException("Invalid origin crypto length.");

            stream.Position = 0;
            stream.Write(_magic);
            
            writeUint32(_version);
            writeUint64(_created_ts);
            writeUint64(_updated_ts);
            writeUint64(_tpm_updated_ts);
            stream.Write(_nonce_origin);
            stream.Write(_nonce_tpm);
            stream.Write(_reserved);
            writeUint32(_len_crypto_origin);
            writeUint32(_len_crypto_tpm);
            stream.Write(_origin_gcm_tag);
            stream.Write(_tpm_gcm_tag);
            if (_len_crypto_origin > 0)
            {
                stream.Write(_crypted_origin);
            }
            stream.SetLength(stream.Position);
            stream.Flush();
        }
        public SCDB(string path) {
            try
            {
                if (File.Exists(path))
                {
                    stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                    ReaderAllFile();
                }
                else
                {
                    createBaseStruct();
                    stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
                    WriterAllFile();
                }
            }catch
            {
                stream?.Dispose();
                throw;
            }
        }
        private void WriteAt(long offset, ReadOnlySpan<byte> data)
        {
            stream.Position = offset;
            stream.Write(data);
        }
        public void setCryptoOrigin(byte[] nonce, byte[] origin_gcm_tag, byte[] data)
        {
            if (nonce.Length != _nonce_origin.Length) {
                throw new ArgumentException("nonce invalid value");
            }
            if (origin_gcm_tag.Length != _origin_gcm_tag.Length) {
                throw new ArgumentException("origin gcm tag none value");
            }
            WriteAt(SCDBLayout.OriginNonceOffset, nonce);
            nonce.CopyTo(_nonce_origin, 0);
            WriteAt(SCDBLayout.OriginTagOffset, origin_gcm_tag);
            origin_gcm_tag.CopyTo(_origin_gcm_tag, 0);
            stream.Position = SCDBLayout.OriginLenOffset;
            writeUint32((uint)data.Length);
            _len_crypto_origin = (uint)data.Length;
            //TODO: Read TPM Crypto
            WriteAt(SCDBLayout.OriginCiphertextOffset, data);
            _crypted_origin = data.ToArray();
            stream.SetLength(stream.Position);
            //TODO: Write TPM Crypto in new offset

            stream.Position = SCDBLayout.UpdateTSOffset;
            ulong update_ts = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            writeUint64(update_ts);
            stream.Flush();
            _updated_ts = update_ts;
        }
        public (byte[] Nonce, byte[] Tag, byte[] Ciphertext) GetOriginBlock()
        {
            return (
                _nonce_origin.ToArray(),
                _origin_gcm_tag.ToArray(),
                _crypted_origin.ToArray()
            );
        }
        public void Dispose() {
            stream?.Dispose();
        }
    }
}
