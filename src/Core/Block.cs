using Core.Crypto;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public readonly struct Block
    {
        public static readonly byte CurrentVersion = 1;
        
        private readonly byte _type;
        private readonly byte _digits; // 6, 8
        private readonly byte _algorithm; // SHA1, SHA256, SHA512
        private readonly ulong _ts_created;
        private readonly ulong _ts_updated;
        private readonly ulong _period_or_counter;
        private readonly uint _len_service_name;
        private readonly uint _len_secret;
        private readonly uint _len_extra;

        private readonly byte[] _service_name;
        private readonly byte[] _secret;
        private readonly byte[] _extra;

        public string NameService {
            get {
                return Encoding.UTF8.GetString(_service_name);
            }
        }

        public BlockTypes Type
        {
            get
            {
                return _type switch
                {
                    1 => BlockTypes.TOTP,
                    _ => BlockTypes.UNKNOWN
                };
            }
        }
        public int Digits {
            get { 
                return (int) _digits;
            }
        }
        public AlgorithmType Algorithm 
        {
            get {
                return _algorithm switch
                {
                    1 => AlgorithmType.SHA1,
                    2 => AlgorithmType.SHA256,
                    3 => AlgorithmType.SHA512,
                    _ => AlgorithmType.Unknown
                };
            }
        }

        private static DateTime convertTS(ulong ts) => DateTimeOffset.FromUnixTimeMilliseconds((long)ts).UtcDateTime;
        public DateTime Created
        {
            get { return convertTS(_ts_created); }
        }
        public DateTime Updated
        {
            get { return convertTS(_ts_updated); }
        }
        public int Code {
            get {
                switch (Type)
                {
                    case BlockTypes.TOTP:
                        var totp = new Crypto.TotpGenerator(_secret, Algorithm, Digits, checked((int)_period_or_counter));
                        return totp.Code;
                    default:
                        throw new InvalidOperationException("Unknown block type");
                }
            }
        }
        public string CodeString 
        {
            get {
                switch (Type)
                {
                    case BlockTypes.TOTP:
                        var totp = new Crypto.TotpGenerator(_secret, Algorithm, Digits, checked((int)_period_or_counter));
                        return totp.CodeString;
                    default:
                        throw new InvalidOperationException("Unknown block type");
                }
            }
        }
        private void writeUint32(uint value, MemoryStream ms)
        {
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf, value);
            ms.Write(buf);
        }
        private void writeUint64(ulong value, MemoryStream ms)
        {
            Span<byte> buf = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(buf, value);
            ms.Write(buf);
        }
        public void WriteTo(MemoryStream ms)
        {
            ms.WriteByte(CurrentVersion);
            ms.WriteByte(_type);
            ms.WriteByte(_digits);
            ms.WriteByte(_algorithm);
            writeUint64(_ts_created, ms);
            writeUint64(_ts_updated, ms);
            writeUint64(_period_or_counter, ms);
            writeUint32(_len_service_name, ms);
            writeUint32(_len_secret, ms);
            writeUint32(_len_extra, ms);

            ms.Write(_service_name);
            ms.Write(_secret);
            ms.Write(_extra);
        }

        public Block(BlockTypes blockType, int digits, AlgorithmType alg, ulong period_or_counter, string service_name, Span<byte> secret) {
            if (digits < 3 || digits > 254)
                throw new ArgumentException("digit is not in the range [3;254]");
            _type = ((byte)blockType);
            _digits = ((byte)digits);
            _algorithm = ((byte)alg);
            _ts_created = _ts_updated = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            _period_or_counter = period_or_counter;
            _service_name = Encoding.UTF8.GetBytes(service_name);
            _len_service_name = (uint)_service_name.Length;
            _len_extra = 0;
            _extra = Array.Empty<byte>();
            _secret = secret.ToArray();
            _len_secret = (uint)_secret.Length;
            CryptographicOperations.ZeroMemory(secret);
        }
        internal Block(byte type, byte digits, byte algorithm, ulong ts_created, ulong ts_updated, ulong period_or_counter, string service_name, Span<byte> secret, byte[] extra)
        {
            _type = type;
            _digits = digits;
            _algorithm = algorithm;
            _ts_created = ts_created;
            _ts_updated = ts_updated;
            _period_or_counter = period_or_counter;
            _service_name = Encoding.UTF8.GetBytes(service_name);
            _len_service_name = (uint)_service_name.Length;
            _secret = secret.ToArray();
            _len_secret = (uint)_secret.Length;
            _extra = extra;
            _len_extra = (uint)_extra.Length;
            CryptographicOperations.ZeroMemory(secret);
        } 
        private Block(byte type, byte digits, byte algorithm, ulong ts_created, ulong ts_updated, ulong period_or_counter, uint len_service_name, uint len_secret, uint len_extra, byte[] service_name, byte[] secret, byte[] extra)
        {
            _type = type;
            _digits = digits;
            _algorithm = algorithm;
            _ts_created = ts_created;
            _ts_updated = ts_updated;
            _period_or_counter = period_or_counter;
            _len_service_name = len_service_name;
            _len_secret = len_secret;
            _len_extra = len_extra;
            _service_name = service_name;
            _secret = secret;
            _extra = extra;
        }
        public Block WithUpdatedTimestamp()
        {
            
            return new Block(
                _type,
                _digits,
                _algorithm,
                _ts_created,
                (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                _period_or_counter,                          
                _len_service_name,
                _len_secret,
                _len_extra,
                _service_name,
                _secret,
                _extra
            );
        }
        public void SecretClear()
        {
            CryptographicOperations.ZeroMemory(_secret);
        }
    }
}
