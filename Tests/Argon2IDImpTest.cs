using Konscious.Security.Cryptography;

namespace Tests.Crypto
{
    [TestClass]
    public class Argon2IDImpTest
    {
        [TestMethod]
        public void StandardVectorTest() 
        {
            byte[] res_tag = {0x0D, 0x64, 0x0D, 0xF5, 0x8D, 0x78, 0x76, 0x6C,
                              0x08, 0xC0, 0x37, 0xA3, 0x4A, 0x8B, 0x53, 0xC9,
                              0xD0, 0x1E, 0xF0, 0x45, 0x2D, 0x75, 0xB6, 0x5E,
                              0xB5, 0x25, 0x20, 0xE9, 0x6B, 0x01, 0xE6, 0x59};
            byte[] password = Enumerable.Repeat((byte)0x01, 32).ToArray();
            Argon2id a2id = new Argon2id(password) 
            {
                Salt = Enumerable.Repeat((byte)0x02, 16).ToArray(),
                MemorySize = 32,
                DegreeOfParallelism =4,
                Iterations = 3,
                KnownSecret = Enumerable.Repeat((byte)0x03, 8).ToArray(),
                AssociatedData = Enumerable.Repeat((byte)0x04, 12).ToArray(),
            };
            byte[] hash = a2id.GetBytes(32);
            CollectionAssert.AreEqual(res_tag, hash);
        }
    }
}
