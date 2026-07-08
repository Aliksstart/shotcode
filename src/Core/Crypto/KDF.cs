using System.Security.Cryptography;

namespace Core.Crypto
{
    internal abstract class KDF : IDisposable
    {
        public abstract int SizePayload { get; }
        private KdfType _type;
        protected byte[] key;
        public Span<byte> Key { get { return key; } }

        public KDF(KdfType type)
        {
            _type = type;
        }

        public void Dispose()
        {
            CryptographicOperations.ZeroMemory(key);
        }
        protected virtual void derivativeKey(ref byte[] password) {
            CryptographicOperations.ZeroMemory(password);
            password = Array.Empty<byte>();
        }
        public virtual void MoveTo(Stream stream) {
            stream.WriteByte((byte)_type);
        }
    }
}
