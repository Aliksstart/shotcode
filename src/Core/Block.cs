using Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public readonly struct Block
    {
        private readonly char _version;
        private readonly char _type;
        private readonly char _digits; // 6, 8
        private readonly char _algorithm; // SHA1, SHA256, SHA512
        private readonly ulong _ts_created;
        private readonly ulong _ts_updated;
        private readonly ulong _period_or_counter;
        private readonly uint _len_service_name;
        private readonly uint _len_secret;
        private readonly uint _len_extra;

        private readonly byte[] _service_name;
        private readonly byte[] _secrets;
        private readonly byte[] _extra;

        public BlockTypes Type
        {
            get
            {
                return _type switch
                {
                    (char)1 => BlockTypes.TOTP,
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
                    (char)1 => AlgorithmType.SHA1,
                    (char)2 => AlgorithmType.SHA256,
                    (char)3 => AlgorithmType.SHA512,
                    _ => AlgorithmType.Unknown
                };
            }
        }

        private static DateTime convertTS(ulong ts) => DateTimeOffset.FromUnixTimeMilliseconds((long)ts).DateTime;
        public DateTime Created
        {
            get { return convertTS(_ts_created); }
        }
        public DateTime Updated
        {
            get { return convertTS(_ts_updated); }
        }

        public Block(BlockTypes blockType, int digits, AlgorithmType alg, ulong period_or_counter, string service_name, Span<byte> secret) {
            if (digits < 3 || digits > 254)
                throw new ArgumentException("digit is not in the range [3;254]");
            _version = ((char)1);
            _type = ((char)blockType);
            _digits = ((char)digits);
            _algorithm = ((char)alg);
            _ts_created = _ts_updated = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            _period_or_counter = period_or_counter;
            _service_name = Encoding.UTF8.GetBytes(service_name);
            _len_service_name = (uint)_service_name.Length;
            _len_extra = 0;
            _extra = Array.Empty<byte>();
            _secrets = secret.ToArray();
            _len_secret = (uint)_secrets.Length;
            CryptographicOperations.ZeroMemory(secret);
        }
        private Block(char version, char type, char digits, char algorithm, ulong ts_created, ulong ts_updated, ulong period_or_counter, uint len_service_name, uint len_secret, uint len_extra, byte[] service_name, byte[] secrets, byte[] extra)
        {
            _version = version;
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
            _secrets = secrets;
            _extra = extra;
        }
        public Block WithUpdatedTimestamp()
        {
            
            return new Block(
                _version,
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
                _secrets,
                _extra
            );
        }
    }
}
