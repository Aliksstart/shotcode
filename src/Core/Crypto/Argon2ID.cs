using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Core.Crypto
{
    internal class Argon2ID : KDF
    {
        public const int PayloadSize = 4 + 4 + 4 + 32;

        public override int SizePayload => PayloadSize;
        public const int DefaultMemorySize = 65536;
        public const int DefaultTimeCost = 3;
        public const int DefaultParallelism = 4;

        int _a2id_time_cost = 0;
        int _a2id_memory_cost = 0;
        int _a2id_parallelism = 0;
        byte[] _a2id_salt = new byte[32];

        private void argumentCheck(ref byte[] password)
        {
            if (password == null || password.Length == 0)
                throw new ArgumentNullException(nameof(password), " is empty.");
        }
        protected override void derivativeKey(ref byte[] password)
        {
            using (Konscious.Security.Cryptography.Argon2id a2id = new Konscious.Security.Cryptography.Argon2id(password)
            {
                Salt = _a2id_salt,
                MemorySize = _a2id_memory_cost,
                DegreeOfParallelism = _a2id_parallelism,
                Iterations = _a2id_time_cost
            }) {
                base.key = a2id.GetBytes(32);
                base.derivativeKey(ref password);
            }
                
        }
        public Argon2ID(ref byte[] password, KdfType type, byte[] payload) : base(type)
        {
            try
            {
                argumentCheck(ref password);
                if (payload.Length != SizePayload)
                    throw new InvalidDataException("Invalid size payload");

                int offset = 0;
                uint a2id_time_cost = BinaryPrimitives.ReadUInt32LittleEndian(
                    payload.AsSpan(offset, 4));
                offset += 4;
                uint a2id_memory_cost = BinaryPrimitives.ReadUInt32LittleEndian(
                    payload.AsSpan(offset, 4));
                offset += 4;
                uint a2id_parallelism = BinaryPrimitives.ReadUInt32LittleEndian(
                    payload.AsSpan(offset, 4));
                offset += 4;

                _a2id_memory_cost = checked((int)a2id_memory_cost);
                _a2id_time_cost = checked((int)a2id_time_cost);
                _a2id_parallelism = checked((int)a2id_parallelism);

                if (_a2id_time_cost != DefaultTimeCost) 
                    throw new InvalidDataException($"Invalid value time cost. Expected {DefaultTimeCost} real {_a2id_time_cost}");

                if (_a2id_memory_cost != DefaultMemorySize)
                    throw new InvalidDataException($"Invalid value memory size cost. Expected {DefaultMemorySize} real {_a2id_memory_cost}");

                if (_a2id_parallelism != DefaultParallelism)
                    throw new InvalidDataException($"Invalid value parallelism. Expected {DefaultParallelism} real {_a2id_parallelism}");

                payload.AsSpan(offset, 32).CopyTo(_a2id_salt);
                derivativeKey(ref password);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(password);
                password = Array.Empty<byte>();
            }
        }
        public Argon2ID(ref byte[] password, KdfType type) : base(type)
        {
            try
            {
                argumentCheck(ref password);
                _a2id_time_cost = DefaultTimeCost;
                _a2id_memory_cost = DefaultMemorySize;
                _a2id_parallelism = DefaultParallelism;
                _a2id_salt = RandomNumberGenerator.GetBytes(32);
                derivativeKey(ref password);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(password);
                password = Array.Empty<byte>();
            }
        }

        public override void MoveTo(Stream stream)
        {
            base.MoveTo(stream);
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf, (uint)SizePayload);
            stream.Write(buf);
            BinaryPrimitives.WriteUInt32LittleEndian(buf, (uint)_a2id_time_cost);
            stream.Write(buf);
            BinaryPrimitives.WriteUInt32LittleEndian(buf, (uint)_a2id_memory_cost);
            stream.Write(buf);
            BinaryPrimitives.WriteUInt32LittleEndian(buf, (uint)_a2id_parallelism);
            stream.Write(buf);
            stream.Write(_a2id_salt);
        }
    }
}
