using Core;
using Core.Crypto;
using System.Text;

namespace Tests
{
    [TestClass]
    public class VaultOriginTest
    {
        private SCDB _db;
        private string _path;
        [TestInitialize]
        public void TestInitialize()
        {
            _path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.scdb");
            _db = new SCDB(_path);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }

        protected async Task WaitForConditionAsync(Func<bool> condition, int timeoutMs = 2000, int pollIntervalMs = 30)
        {
            var start = DateTime.UtcNow;

            while (!condition.Invoke())
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                    Assert.Fail($"The condition was not met within {timeoutMs} ms.");

                await Task.Delay(pollIntervalMs);
            }
        }

        private byte[] GetDefaultPassword() {
            return Encoding.UTF8.GetBytes("defaultPassword");
        }
        [TestMethod]
        public void WorkIsEmptyVault_Test() {
            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            vaultOrigin.Save();
            vaultOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => {
                string[] services = vaultOrigin.GetNameServices();
            });
            Assert.ThrowsException<InvalidOperationException>(() => {
                vaultOrigin.GetCode("Test_service");
            });
        }
        [TestMethod]
        public void CheckSaveLocked_Test()
        {
            DateTime updateDt;

            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            updateDt = _db.Updated;
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            vaultOrigin.AddBlock(block);
            vaultOrigin.Save();
            vaultOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => { vaultOrigin.Save(); });
        }
        [TestMethod]
        public void Constructor_CreationTest() 
        {
            DateTime updateDt;
            
            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            updateDt = _db.Updated;
            byte[] secret = { 1, 2, 3 };    
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            vaultOrigin.AddBlock(block);
            vaultOrigin.Save();
            vaultOrigin.Close();
            Assert.AreNotEqual(updateDt, _db.Updated);

            var eq = Assert.ThrowsException<FormatException>(() => {using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { }); });
            Assert.AreEqual("It is not possible to create VaultOrigin in a non-empty database.", eq.Message);

            byte[] local_pass2 = GetDefaultPassword();
            using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass2, 6000, () => { });
            Assert.AreEqual(1, vaultOrigin2.GetNameServices().Length);
            Assert.AreEqual(-1, vaultOrigin2.GetCode("Test_service"));
            
        }
        [TestMethod]
        public void ListServices_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            vaultOrigin.AddBlock(block);
            vaultOrigin.Save();
            vaultOrigin.Close();
            byte[] local_pass2 = GetDefaultPassword();
            using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass2, 60000, () => { });
            string[] ns = vaultOrigin2.GetNameServices();
            Assert.AreEqual(1, ns.Length);
            Assert.AreEqual("service", ns[0]);
        }
        [TestMethod]
        public void AddedBlock_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            Assert.IsTrue(vaultOrigin.AddBlock(block));
            vaultOrigin.Save();
            vaultOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => { vaultOrigin.AddBlock(block); });

            local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass, 60000, () => { });
            Assert.IsFalse(vaultOrigin2.AddBlock(block));
            Assert.AreNotEqual(-1, vaultOrigin2.GetCode("service"));
            Assert.AreEqual(-1, vaultOrigin2.GetCode("service2"));
        }
        [TestMethod]
        public void RemoveBlock_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            Block block2 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service2", secret);
            Assert.IsTrue(vaultOrigin.AddBlock(block));
            vaultOrigin.Save();
            vaultOrigin.Close();

            local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass, 60000, () => { });
            Assert.IsFalse(vaultOrigin2.RemoveBlock("service2"));
            Assert.IsTrue(vaultOrigin2.RemoveBlock("service"));
            Assert.AreEqual(-1, vaultOrigin2.GetCode("service"));
            vaultOrigin2.Save();
            vaultOrigin2.Close();

            local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin3 = new VaultOrigin(_db, ref local_pass, 60000, () => { });
            string[] ns = vaultOrigin3.GetNameServices();
            Assert.AreEqual(0, ns.Length);
        }
        [TestMethod]
        public async Task AutoClose_Test()
        {
            bool auto_closed = false;
            int locked_count = 0;

            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            vaultOrigin.Save();
            vaultOrigin.Close();

            local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin2 = new VaultOrigin(_db, ref local_pass, 300, () => { auto_closed = true; locked_count++; });
            await WaitForConditionAsync(() => auto_closed, timeoutMs: 2000, pollIntervalMs: 30);
            Assert.IsTrue(auto_closed);
            Assert.AreEqual(1, locked_count);
        }
        [TestMethod]
        public async Task StartEstablishedTimerSave_Test()
        {
            bool auto_closed = false;
            int locked_count = 0;

            byte[] local_pass = GetDefaultPassword();
            using VaultOrigin vaultOrigin = new VaultOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { auto_closed = true; locked_count++; });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            vaultOrigin.Save();

            await WaitForConditionAsync(() => auto_closed, timeoutMs: 2000, pollIntervalMs: 30);
            Assert.IsTrue(auto_closed);
            Assert.AreEqual(1, locked_count);
        }
    }
}
