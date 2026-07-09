using Core;
using Core.Crypto;
using System.Text;

namespace Tests
{
    [TestClass]
    public class ValutOriginTest
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
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            valutOrigin.Save();
            valutOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => {
                string[] services = valutOrigin.GetNameServices();
            });
            Assert.ThrowsException<InvalidOperationException>(() => {
                valutOrigin.GetCode("Test_service");
            });
        }
        [TestMethod]
        public void CheckSaveLocked_Test()
        {
            DateTime updateDt;

            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            updateDt = _db.Updated;
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            valutOrigin.AddBlock(block);
            valutOrigin.Save();
            valutOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => { valutOrigin.Save(); });
        }
        [TestMethod]
        public void Constructor_CreationTest() 
        {
            DateTime updateDt;
            
            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            updateDt = _db.Updated;
            byte[] secret = { 1, 2, 3 };    
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            valutOrigin.AddBlock(block);
            valutOrigin.Save();
            valutOrigin.Close();
            Assert.AreNotEqual(updateDt, _db.Updated);

            var eq = Assert.ThrowsException<FormatException>(() => {using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { }); });
            Assert.AreEqual("It is not possible to create ValutOrigin in a non-empty database.", eq.Message);

            byte[] local_pass2 = GetDefaultPassword();
            using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass2, 6000, () => { });
            Assert.AreEqual(1, valutOrigin2.GetNameServices().Length);
            Assert.AreEqual(-1, valutOrigin2.GetCode("Test_service"));
            
        }
        [TestMethod]
        public void ListServices_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            valutOrigin.AddBlock(block);
            valutOrigin.Save();
            valutOrigin.Close();
            byte[] local_pass2 = GetDefaultPassword();
            using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass2, 60000, () => { });
            string[] ns = valutOrigin2.GetNameServices();
            Assert.AreEqual(1, ns.Length);
            Assert.AreEqual("service", ns[0]);
        }
        [TestMethod]
        public void AddedBlock_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            Assert.IsTrue(valutOrigin.AddBlock(block));
            valutOrigin.Save();
            valutOrigin.Close();
            Assert.ThrowsException<InvalidOperationException>(() => { valutOrigin.AddBlock(block); });

            local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass, 60000, () => { });
            Assert.IsFalse(valutOrigin2.AddBlock(block));
            Assert.AreNotEqual(-1, valutOrigin2.GetCode("service"));
            Assert.AreEqual(-1, valutOrigin2.GetCode("service2"));
        }
        [TestMethod]
        public void RemoveBlock_Test()
        {
            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 60000, () => { });
            byte[] secret = { 1, 2, 3 };
            Block block = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service", secret);
            Block block2 = new Block(BlockTypes.TOTP, 6, AlgorithmType.SHA1, 30, "service2", secret);
            Assert.IsTrue(valutOrigin.AddBlock(block));
            valutOrigin.Save();
            valutOrigin.Close();

            local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass, 60000, () => { });
            Assert.IsFalse(valutOrigin2.RemoveBlock("service2"));
            Assert.IsTrue(valutOrigin2.RemoveBlock("service"));
            Assert.AreEqual(-1, valutOrigin2.GetCode("service"));
            valutOrigin2.Save();
            valutOrigin2.Close();

            local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin3 = new ValutOrigin(_db, ref local_pass, 60000, () => { });
            string[] ns = valutOrigin3.GetNameServices();
            Assert.AreEqual(0, ns.Length);
        }
        [TestMethod]
        public async Task AutoClose_Test()
        {
            bool auto_closed = false;
            int locked_count = 0;

            byte[] local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            valutOrigin.Save();
            valutOrigin.Close();

            local_pass = GetDefaultPassword();
            using ValutOrigin valutOrigin2 = new ValutOrigin(_db, ref local_pass, 300, () => { auto_closed = true; locked_count++; });
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
            using ValutOrigin valutOrigin = new ValutOrigin(_db, ref local_pass, KdfType.Argon2id, 10, () => { auto_closed = true; locked_count++; });
            Assert.AreSame(local_pass, Array.Empty<byte>());
            valutOrigin.Save();

            await WaitForConditionAsync(() => auto_closed, timeoutMs: 2000, pollIntervalMs: 30);
            Assert.IsTrue(auto_closed);
            Assert.AreEqual(1, locked_count);
        }
    }
}
