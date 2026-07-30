using Core;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Tests
{
    [TestClass]
    public class SCDBTests
    {
        [TestMethod]
        public void DoubleOpenFile()
        {
            string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");

            try
            {
                using (var db = new SCDB(path))
                {
                    Assert.IsTrue(File.Exists(path));
                    Assert.IsTrue(new FileInfo(path).Length > 0);
                    Assert.ThrowsException<IOException>(() => { using var db2 = new SCDB(path); });
                }
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        [TestMethod]
        public void CreateNewFile_ThenRead_ShouldBeValid()
        {
            string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");
            
            try
            {
                DateTime ctime = DateTime.UtcNow;
                using (var db = new SCDB(path))
                {
                }

                Assert.IsTrue(File.Exists(path));
                Assert.IsTrue(new FileInfo(path).Length > 0);

                using (var db = new SCDB(path))
                {
                    Assert.IsTrue(db.IsClear());
                    Assert.IsTrue((db.Created - ctime).Duration() < TimeSpan.FromSeconds(2));
                    Assert.IsTrue((db.Updated - ctime).Duration() < TimeSpan.FromSeconds(2));
                }
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        [TestMethod]
        public void InvalidVersion_ShouldThrow()
        {
            string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");

            try
            {
                using (var db = new SCDB(path))
                {
                }

                Span<byte> buf = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf, 5);

                using (Stream fs = File.Open(path, FileMode.Open, FileAccess.Write))
                {
                    fs.Position = 4;
                    fs.Write(buf);
                }

                Assert.ThrowsException<InvalidDataException>(() =>
                {
                    using var db = new SCDB(path);
                });
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        [TestMethod]
        public void SetCryptoOrigin_ShouldWriteBlockAndUpdateTimestamp()
        {
            string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");

            try
            {
                byte[] nonce = RandomNumberGenerator.GetBytes(SCDBLayout.NonceSize);
                byte[] tag = RandomNumberGenerator.GetBytes(SCDBLayout.GcmTagSize);
                byte[] ciphertext = RandomNumberGenerator.GetBytes(32);
                DateTime oldUpdated;
                using (var db = new SCDB(path))
                {

                    oldUpdated = db.Updated;

                    db.setCryptoOrigin(nonce, tag, ciphertext);

                    var (origReadNonce, origRreadTag, origReadCipher) = db.GetOriginBlock();

                    CollectionAssert.AreEqual(nonce, origReadNonce, "Nonce didn't match");
                    CollectionAssert.AreEqual(tag, origRreadTag, "GCM tag didn't match");
                    CollectionAssert.AreEqual(ciphertext, origReadCipher, "Ciphertext didn't match");
                }

                using var db2 = new SCDB(path);

                var (readNonce2, readTag2, readCipher2) = db2.GetOriginBlock();

                CollectionAssert.AreEqual(nonce, readNonce2, "Nonce after restart it didn't match");
                CollectionAssert.AreEqual(tag, readTag2, "GCM tag after restart it didn't match");
                CollectionAssert.AreEqual(ciphertext, readCipher2, "Ciphertext after restart it didn't match");

                Assert.IsTrue(db2.Updated > oldUpdated, "Updated timestamp should have increased after writing crypto block");
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}