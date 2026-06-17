using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Core.Crypto
{
    public class TotpGenerator
    {
        int _digits;
        private static readonly int[] PowersOfTen = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 };
        private void StandardApply(byte[] secret, AlgorithmType algorithm, int digits, int period, int t0 = 0) 
        {
            if (secret == null)
                throw new ArgumentNullException("Secret is null");
            if (secret.Length == 0)
                throw new ArgumentException("Secret must not be empty.", nameof(secret));
            if (digits < 4 || digits > 8)
                throw new ArgumentOutOfRangeException("Digits must be between 4 and 8.");
            if (period < 15 || period > 120)
                throw new ArgumentOutOfRangeException("Period must be between 15 and 120 seconds.");
            if (t0 < 0)
                throw new ArgumentOutOfRangeException(nameof(t0));
            _digits = digits;
        }
        private void Calculation(byte[] secret, AlgorithmType algorithm, int period, long unix_time, int t0)
        {
            using HMAC hmac = algorithm switch
            {
                AlgorithmType.SHA1 => new HMACSHA1(secret),
                AlgorithmType.SHA256 => new HMACSHA256(secret),
                AlgorithmType.SHA512 => new HMACSHA512(secret),
                _ => throw new InvalidOperationException(nameof(algorithm))
            };
            byte[] buf = new byte[8];
            long t = (unix_time - t0) / period;
            BinaryPrimitives.WriteInt64BigEndian(buf, t);
            byte[] hash = hmac.ComputeHash(buf);
            int offset = hash[^1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

            Code = binary % PowersOfTen[_digits];
        }

        public TotpGenerator(byte[] secret, AlgorithmType algorithm, int digits, int period, int t0 = 0) {
            
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= t0)
                throw new ArgumentException("Invalid value t0");
            StandardApply(secret, algorithm, digits, period, t0);
            Calculation(secret, algorithm, period, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), t0);
        }

        public TotpGenerator(byte[] secret, AlgorithmType algorithm, int digits, int period, long unix_time, int t0 = 0)
        {
            if (unix_time < 0)
                throw new ArgumentOutOfRangeException(nameof(unix_time));
            if (unix_time < t0)
                throw new ArgumentOutOfRangeException("Invalid value t0");
            StandardApply(secret, algorithm, digits, period, t0);
            Calculation(secret, algorithm, period, unix_time, t0);
            
        }

        public int Code
        {
            get; private set;
        }
        public string CodeString
        {
            get
            {
                return Code.ToString($"D{_digits}");
            }
        }
    }
}
