using Core.Crypto;
using System.Text;

namespace Tests.Crypto
{
    [TestClass]
    public class Base32Tests
    {
        /*
        BASE32("") = ""
        BASE32("f") = "MY======"
        BASE32("fo") = "MZXQ===="
        BASE32("foo") = "MZXW6==="
        BASE32("foob") = "MZXW6YQ="
        BASE32("fooba") = "MZXW6YTB"
        BASE32("foobar") = "MZXW6YTBOI======"
        */
        byte[] r_0 = { };
        byte[] r_1 = Encoding.UTF8.GetBytes("f");
        byte[] r_2 = Encoding.UTF8.GetBytes("fo");
        byte[] r_3 = Encoding.UTF8.GetBytes("foo");
        byte[] r_4 = Encoding.UTF8.GetBytes("foob");
        byte[] r_5 = Encoding.UTF8.GetBytes("fooba");
        byte[] r_6 = Encoding.UTF8.GetBytes("foobar");
        string l_0 = "";
        string l_1 = "MY======";
        string l_2 = "MZXQ====";
        string l_3 = "MZXW6===";
        string l_4 = "MZXW6YQ=";
        string l_5 = "MZXW6YTB";
        string l_6 = "MZXW6YTBOI======";

        private void EncHelper(byte[] a, string b)
        {
            Span<char> dst = stackalloc char[Base32.GetEncodeLength(a.Length)];
            Base32.Encode(a, dst);
            Assert.AreEqual(b, dst.ToString());
        }
        private void DecHelper(string a, byte[]? b)
        {
            Span<byte> dst = stackalloc byte[Base32.GetDecodeLength(a)];
            Base32.Decode(a, dst);
            if (b != null)
            {
                CollectionAssert.AreEqual(b, dst.ToArray());
            }
        }
        [TestMethod]
        public void StandartEncodeTest()
        {
            EncHelper(r_0, l_0);
            EncHelper(r_1, l_1);
            EncHelper(r_2, l_2);
            EncHelper(r_3, l_3);
            EncHelper(r_4, l_4);
            EncHelper(r_5, l_5);
            EncHelper(r_6, l_6);
        }
        [TestMethod]
        public void StandartDecodeTest()
        {
            DecHelper(l_0, r_0);
            DecHelper(l_1, r_1);
            DecHelper(l_2, r_2);
            DecHelper(l_3, r_3);
            DecHelper(l_4, r_4);
            DecHelper(l_5, r_5);
            DecHelper(l_6, r_6);
        }
        [TestMethod]
        public void RoundTripTest()
        {
            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                int s = rnd.Next(32, 1024);
                byte[] d = new byte[s];
                rnd.NextBytes(d);
                Span<char> d_enc = stackalloc char[Base32.GetEncodeLength(d.Length)];
                Base32.Encode(d, d_enc);
                Span<byte> dec = stackalloc byte[d.Length];
                Base32.Decode(d_enc, dec);
                CollectionAssert.AreEqual(d, dec.ToArray());
            }
        }
        [TestMethod]
        public void InvalidDataExceptionGetDecodeLenghtTest()
        {
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ==AR=";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ=A==";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ==";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ=====";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ====\n";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZX=====";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "M=======";
                Base32.GetDecodeLength(str);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "========";
                Base32.GetDecodeLength(str);
            });
        }
        [TestMethod]
        public void InvalidDataExceptionDecodeTest()
        {
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "";
                Span<byte> d = stackalloc byte[4];
                Base32.Decode(str.AsSpan(), d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MY======";
                Span<byte> d = stackalloc byte[0];
                Base32.Decode(str.AsSpan(), d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ====MY======";
                Span<byte> d = stackalloc byte[0];
                Base32.Decode(str.AsSpan(), d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ====MY======";
                Span<byte> d = stackalloc byte[Base32.GetDecodeLength(str)];
                Base32.Decode(str, d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXQ==A=";
                Span<byte> d = stackalloc byte[Base32.GetDecodeLength(str)];
                Base32.Decode(str, d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "mzxq====";
                Span<byte> d = stackalloc byte[Base32.GetDecodeLength(str)];
                Base32.Decode(str, d);
            });
            Assert.ThrowsException<InvalidDataException>(() => {
                string str = "MZXR====";
                Span<byte> d = stackalloc byte[Base32.GetDecodeLength(str)];
                Base32.Decode(str, d);
            });
        }
    }
}
