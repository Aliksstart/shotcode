using Core.Crypto;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Crypto
{
    [TestClass]
    public class TOTPTest
    {
        [TestMethod]
        public void StandardVectorTest()
        {
            byte[] secretSha1 = Encoding.UTF8.GetBytes("12345678901234567890");
            byte[] secretSha256 = Encoding.ASCII.GetBytes("12345678901234567890123456789012");
            byte[] secretSha512 = Encoding.ASCII.GetBytes(
                "1234567890123456789012345678901234567890123456789012345678901234");
            int t0 = 0;
            int period = 30;
            long time_test_1_2_3 = 59;

            TotpGenerator gen_test1 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_test_1_2_3, t0);
            Assert.AreEqual(94287082, gen_test1.Code);
            TotpGenerator gen_test2 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_test_1_2_3, t0);
            Assert.AreEqual(46119246, gen_test2.Code);
            TotpGenerator gen_test3 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_test_1_2_3, t0);
            Assert.AreEqual(90693936, gen_test3.Code);
            long time_test_4_5_6 = 1111111109;
            TotpGenerator gen_test4 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_test_4_5_6, t0);
            Assert.AreEqual("07081804", gen_test4.CodeString);
            TotpGenerator gen_test5 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_test_4_5_6, t0);
            Assert.AreEqual(68084774, gen_test5.Code);
            TotpGenerator gen_test6 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_test_4_5_6, t0);
            Assert.AreEqual(25091201, gen_test6.Code);
            long time_test_7_8_9 = 1111111111;
            TotpGenerator gen_test7 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_test_7_8_9, t0);
            Assert.AreEqual(14050471, gen_test7.Code);
            TotpGenerator gen_test8 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_test_7_8_9, t0);
            Assert.AreEqual(67062674, gen_test8.Code);
            TotpGenerator gen_test9 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_test_7_8_9, t0);
            Assert.AreEqual(99943326, gen_test9.Code);
            long time_10_11_12 = 1234567890;
            TotpGenerator gen_test10 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_10_11_12, t0);
            Assert.AreEqual(89005924, gen_test10.Code);
            TotpGenerator gen_test11 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_10_11_12, t0);
            Assert.AreEqual(91819424, gen_test11.Code);
            TotpGenerator gen_test12 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_10_11_12, t0);
            Assert.AreEqual(93441116, gen_test12.Code);
            long time_13_14_15 = 2000000000;
            TotpGenerator gen_test13 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_13_14_15, t0);
            Assert.AreEqual(69279037, gen_test13.Code);
            TotpGenerator gen_test14 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_13_14_15, t0);
            Assert.AreEqual(90698825, gen_test14.Code);
            TotpGenerator gen_test15 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_13_14_15, t0);
            Assert.AreEqual(38618901, gen_test15.Code);
            long time_16_17_18 = 20000000000;
            TotpGenerator gen_test16 = new TotpGenerator(secretSha1, AlgorithmType.SHA1, 8, period, time_16_17_18, t0);
            Assert.AreEqual(65353130, gen_test16.Code);
            TotpGenerator gen_test17 = new TotpGenerator(secretSha256, AlgorithmType.SHA256, 8, period, time_16_17_18, t0);
            Assert.AreEqual(77737706, gen_test17.Code);
            TotpGenerator gen_test18 = new TotpGenerator(secretSha512, AlgorithmType.SHA512, 8, period, time_16_17_18, t0);
            Assert.AreEqual(47863826, gen_test18.Code);
        }
        [TestMethod]
        public void GuardClassesTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TotpGenerator(null!, AlgorithmType.SHA1, 8, 30, 59, 0));
            Assert.ThrowsException<ArgumentException>(() => new TotpGenerator(Array.Empty<byte>(), AlgorithmType.SHA1, 8, 30, 59, 0));
            byte[] secret = Encoding.UTF8.GetBytes("12345678901234567890");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 0, 30, 59, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 9, 30, 59, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 8, 0, 59, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 8, 121, 59, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 8, 30, -1, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TotpGenerator(secret, AlgorithmType.SHA1, 8, 30, 59, -1));
            Assert.ThrowsException<InvalidOperationException>(() => new TotpGenerator(secret, AlgorithmType.Unknown, 8, 30, 59, 0));
        }
        [TestMethod]
        public void RealTimeTest() {
            byte[] secret = Encoding.UTF8.GetBytes("12345678901234567890");
            int period = 30;
            int digits = 8;
            var hmac = new HMACSHA1(secret);
            long pre_T = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / period;
            TotpGenerator gen = new TotpGenerator(secret, AlgorithmType.SHA1, digits, period);
            byte[] T = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(T, pre_T);
            byte[] hash = hmac.ComputeHash(T);

            int offset = hash[^1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);
            int val = binary % 100000000;
            Assert.AreEqual(val.ToString($"D{digits}"), gen.CodeString);
        }
    }
}
