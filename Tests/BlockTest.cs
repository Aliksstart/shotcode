using Core;
using Core.Crypto;

namespace Tests
{
    [TestClass]
    public class BlockTest
    {
        [TestMethod]
        public void BlockTypeGetter_ShouldReturnCorrectEnum()
        {
            byte[] tmp = Array.Empty<byte>();
            Block b1 = new Block(BlockTypes.TOTP, (char)6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(BlockTypes.TOTP, b1.Type);

            var bUnknown = new Block((BlockTypes)255, (char)6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(BlockTypes.UNKNOWN, bUnknown.Type);
        }
        [TestMethod]
        public void AlgorithmGetter_ShouldReturnCorrectEnum()
        {
            byte[] tmp = Array.Empty<byte>();
            Block b1 = new Block(BlockTypes.TOTP, (char)6, AlgorithmType.SHA256, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA256, b1.Algorithm);
            Block b2 = new Block(BlockTypes.TOTP, (char)6, AlgorithmType.SHA1, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA1, b2.Algorithm);
            Block b3 = new Block(BlockTypes.TOTP, (char)6, AlgorithmType.SHA512, 30, "service", Array.Empty<byte>());
            Assert.AreEqual(AlgorithmType.SHA512, b3.Algorithm);

            var bUnknown = new Block(BlockTypes.TOTP, (char)6, (AlgorithmType)255, 30, "service", Array.Empty<byte>());
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

            Assert.IsTrue(bUpdated.Updated > initialTs, "Новый таймстамп должен быть больше оригинального");

            Assert.AreEqual(bOriginal.Type, bUpdated.Type);
            Assert.AreEqual(bOriginal.Digits, bUpdated.Digits);
            Assert.AreEqual(bOriginal.Algorithm, bUpdated.Algorithm);
        }


    }
}
