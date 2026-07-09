using Core;
using Core.Crypto;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Tests
{
    [TestClass]
    public class BlockTest
    {
        [TestMethod]
        public void NameServiceGetter_ShouldReturnCorrect()
        {
            byte[] tmp = Array.Empty<byte>();
            Block b1 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual("service", b1.NameService);
        }
        [TestMethod]
        public void TOTPCodeGetter_ShouldReturnCorrect()
        {
            byte[] secret = Encoding.UTF8.GetBytes("12345678901234567890");
            int period = 30;
            int digits = 8;
            var hmac = new HMACSHA1(secret);
            long pre_T = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / period;
            Block b1 = new Block(BlockTypes.TOTP, digits, AlgorithmType.SHA1, (ulong)period, "test", secret);
            byte[] T = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(T, pre_T);
            byte[] hash = hmac.ComputeHash(T);

            int offset = hash[^1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);
            int val = binary % 100000000;
            Assert.AreEqual(val.ToString($"D{digits}"), b1.CodeString);
            Assert.AreEqual(val, b1.Code);
        }
        [TestMethod]
        public void BlockTypeGetter_ShouldReturnCorrectEnum()
        {
            byte[] tmp = Array.Empty<byte>();
            Block b1 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(BlockTypes.TOTP, b1.Type);

            var bUnknown = new Block((BlockTypes)255, 6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(BlockTypes.UNKNOWN, bUnknown.Type);
        }
        [TestMethod]
        public void AlgorithmGetter_ShouldReturnCorrectEnum()
        {
            byte[] tmp = Array.Empty<byte>();
            Block b1 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA256, b1.Algorithm);
            Block b2 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA1, b2.Algorithm);
            Block b3 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA512, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA512, b3.Algorithm);

            var bUnknown = new Block(BlockTypes.TOTP, 6, (AlgorithmType)255, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.Unknown, bUnknown.Algorithm);
        }
        [TestMethod]
        public void Digits_ShouldAcceptValidRange()
        {
            Block bMin = new Block(BlockTypes.TOTP, 3, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(3, bMin.Digits);

            Block bMax = new Block(BlockTypes.TOTP, 254, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(254, bMax.Digits);
        }

        [TestMethod]
        public void Digits_ShouldThrowArgumentException_WhenValueIsInvalid()
        {
            var exMin = Assert.ThrowsException<ArgumentException>(() =>
            {
                new Block(BlockTypes.TOTP, 2, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            });
            Assert.AreEqual("digit is not in the range [3;254]", exMin.Message);

            var exMax = Assert.ThrowsException<ArgumentException>(() =>
            {
                new Block(BlockTypes.TOTP, 255, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            });
            Assert.AreEqual("digit is not in the range [3;254]", exMax.Message);
        }
        [TestMethod]
        public void Constructor_ShouldClearSourceSecretBytes_AfterCopying()
        {
            byte[] originalSecret = { 1, 2, 3, 4, 5 };

            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", originalSecret);

            foreach (byte b in originalSecret)
            {
                Assert.AreEqual(0, b, "original secret is not empty");
            }
        }

        [TestMethod]
        public async Task WithUpdatedTimestamp_ShouldReturnNewInstance_WithNewTime()
        {
            Block bOriginal = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            DateTime initialTs = bOriginal.Created;

            await Task.Delay(5);

            Block bUpdated = bOriginal.WithUpdatedTimestamp();

            Assert.AreEqual(initialTs, bOriginal.Created);

            Assert.IsTrue(bUpdated.Updated > initialTs, "The new timestamp must be larger than the original");

            Assert.AreEqual(bOriginal.Type, bUpdated.Type);
            Assert.AreEqual(bOriginal.Digits, bUpdated.Digits);
            Assert.AreEqual(bOriginal.Algorithm, bUpdated.Algorithm);
        }

        [TestMethod]
        public void WriteTo_ShouldWriteCorrectBinaryLayout()
        {
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1,30, "service", secret);

            using MemoryStream ms = new MemoryStream();

            block.WriteTo(ms);

            byte[] data = ms.ToArray();

            Assert.AreEqual(Block.CurrentVersion, data[BlockLayout.VersionOffset]);
            Assert.AreEqual((byte)BlockTypes.TOTP, data[BlockLayout.TypeOffset]);
            Assert.AreEqual(6, data[BlockLayout.DigitsOffset]);
            Assert.AreEqual((byte)AlgorithmType.SHA1, data[BlockLayout.AlgorithmOffset]);

            uint serviceLen = BinaryPrimitives.ReadUInt32LittleEndian(
                data.AsSpan(BlockLayout.ServiceNameLengthOffset, BlockLayout.LengthSize));

            uint secretLen = BinaryPrimitives.ReadUInt32LittleEndian(
                data.AsSpan(BlockLayout.SecretLengthOffset, BlockLayout.LengthSize));

            uint extraLen = BinaryPrimitives.ReadUInt32LittleEndian(
                data.AsSpan(BlockLayout.ExtraLengthOffset, BlockLayout.LengthSize));

            Assert.AreEqual((uint)Encoding.UTF8.GetByteCount("service"), serviceLen);
            Assert.AreEqual((uint)3, secretLen);
            Assert.AreEqual((uint)0, extraLen);
        }
    }
}
