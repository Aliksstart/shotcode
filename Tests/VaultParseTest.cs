using Core;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Tests
{
    public class TestVault : Core.Vault
    {
        public static readonly byte[] Key = new byte[32];

        public TestVault(SCDB db, int interval, Action lockedEvent, VaultState vs) : base(db, interval, lockedEvent, vs)
        {
        }

        public void OpenPlaintext(byte[] plaintext)
        {
            byte[] nonce = new byte[SCDBLayout.NonceSize];
            byte[] tag = new byte[SCDBLayout.GcmTagSize];
            byte[] ciphertext = new byte[plaintext.Length];
            using (AesGcm aes = new AesGcm(Key, tag.Length))
            {
                aes.Encrypt(nonce, plaintext, ciphertext, tag);
            }
            base.Open(Key, nonce, tag, ciphertext);
        }

        protected override void OnLocking()
        {
            return;
        }
    }
    [TestClass]
    public class VaultParseTest
    {
        private SCDB _db;
        private string _path;
        private TestVault _vault;
        [TestInitialize]
        public void TestInitialize()
        {
            _path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");
            _db = new SCDB(_path);
            _vault = new TestVault(_db, 10000, () => { }, VaultState.Locked);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            _vault.Dispose();
            _db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }

        private static byte[] MakeHeader(
            byte version = 1,
            byte type = 1,
            byte digits = 6,
            byte algorithm = 1,
            ulong created = 0,
            ulong updated = 0,
            ulong periodOrCounter = 30,
            uint lenServiceName = 1,
            uint lenSecret = 1,
            uint lenExtra = 0,
            byte[] dat = null)
        {
            byte[] header = dat == null ? new byte[BlockLayout.MinBlockSize] : new byte[BlockLayout.MinBlockSize + dat.Length];
            header[BlockLayout.VersionOffset] = version;
            header[BlockLayout.TypeOffset] = type;
            header[BlockLayout.DigitsOffset] = digits;
            header[BlockLayout.AlgorithmOffset] = algorithm;
            BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(BlockLayout.CreatedTSOffset), created);
            BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(BlockLayout.UpdatedTSOffset), updated);
            BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(BlockLayout.PeriodOrCounterOffset), periodOrCounter);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(BlockLayout.ServiceNameLengthOffset), lenServiceName);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(BlockLayout.SecretLengthOffset), lenSecret);
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(BlockLayout.ExtraLengthOffset), lenExtra);
            if (dat != null)
            {
                dat.CopyTo(header, BlockLayout.MinBlockSize);
            }
            return header;
        }

        private static byte[] MakePayload(uint count, params byte[][] chunks)
        {
            using MemoryStream ms = new MemoryStream();
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buf, count);
            ms.Write(buf);
            foreach (byte[] chunk in chunks)
                ms.Write(chunk);
            return ms.ToArray();
        }

        [TestMethod]
        public void CheckThrow_PayloadCorruption()
        {
            byte[] b = { 0x05 };
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(b); });
        }
        [TestMethod]
        public void CheckThrow_InvalidBlockSize()
        {
            Span<byte> b = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(b, 1);
            byte[] t = b.ToArray();
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(t); });
        }
        [TestMethod]
        public void CheckThrow_InvalidVersion()
        {
            byte[] payload = MakePayload(1, MakeHeader(byte.MaxValue));
            var e = Assert.ThrowsException<NotSupportedException>(() => { _vault.OpenPlaintext(payload); });
        }
        [TestMethod]
        public void CheckThrow_InvalidLenExtra()
        {
            byte[] payload = MakePayload(1, MakeHeader(lenExtra: 1));
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(payload); });
        }
        [TestMethod]
        public void CheckThrow_InvalidLenServiceNameIsZero()
        {
            byte[] payload = MakePayload(1, MakeHeader(lenServiceName: 0));
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(payload); });
        }
        [TestMethod]
        public void CheckThrow_Invalid_Offset_LenServiceName()
        {
            byte[] payload = MakePayload(1, MakeHeader(lenServiceName: 10));
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(payload); });
        }
        [TestMethod]
        public void CheckThrow_InvalidLenSecretIsZero()
        {
            byte[] service_name = Encoding.UTF8.GetBytes("service");
            byte[] b = MakeHeader(lenServiceName: (uint)service_name.Length, lenSecret: 0, dat: service_name);
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(MakePayload(1, b)); });
        }
        [TestMethod]
        public void CheckThrow_Invalid_Offset_LenSecret()
        {
            byte[] service_name = Encoding.UTF8.GetBytes("service");
            byte[] b = MakeHeader(lenServiceName: (uint)service_name.Length, lenSecret: 10, dat: service_name);
            Assert.ThrowsException<InvalidDataException>(() => { _vault.OpenPlaintext(MakePayload(1, b)); });
        }
    }
}
