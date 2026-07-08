using Core.Crypto;
using Konscious.Security.Cryptography;
using System.Buffers.Binary;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace Tests.Crypto
{
    [TestClass]
    public class Argon2IDTest
    {
        private int SizePreDataPayload => 5;
        private byte[] GetDefaultPassword()
        { 
            return Encoding.UTF8.GetBytes("defaultPassword");
        }
        [TestMethod]
        public void Argon2ID_EmptyPasswordTest()
        {
            byte[] password_null = null;
            Assert.ThrowsException<ArgumentNullException>(() => { var kdf = new Argon2ID(ref password_null, KdfType.Argon2id); });

            byte[] password_empty =  Array.Empty<byte>();
            Assert.ThrowsException<ArgumentNullException>(() => { var kdf = new Argon2ID(ref password_empty, KdfType.Argon2id); });
        }
        [TestMethod]
        public void Argon2ID_PayloadTest()
        {
            byte[] local_pass1 = GetDefaultPassword();
            using var control_kdf = new Argon2ID(ref local_pass1, Core.Crypto.KdfType.Argon2id);
            Assert.AreEqual(local_pass1, Array.Empty<byte>());

            MemoryStream ms = new MemoryStream();
            control_kdf.MoveTo(ms);
            byte[] buffer = ms.ToArray();
            byte[] defaultPayload = new byte[control_kdf.SizePayload];
            buffer.AsSpan(SizePreDataPayload, control_kdf.SizePayload).CopyTo(defaultPayload);

            {
                byte[] pass = GetDefaultPassword();
                using var kdf = new Argon2ID(ref pass, KdfType.Argon2id, defaultPayload);
                CollectionAssert.AreEqual(control_kdf.Key.ToArray(), kdf.Key.ToArray());
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, UInt32.MaxValue);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(4, defaultPayload.Length - 4));
                var eq = Assert.ThrowsException<OverflowException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                ms_custom_default_value.Write(defaultPayload.AsSpan(0, 4));
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, UInt32.MaxValue);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(8, defaultPayload.Length - 8));
                var eq = Assert.ThrowsException<OverflowException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                ms_custom_default_value.Write(defaultPayload.AsSpan(0, 8));
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, UInt32.MaxValue);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(12, defaultPayload.Length - 12));
                var eq = Assert.ThrowsException<OverflowException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, Argon2ID.DefaultTimeCost + 1);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(4, defaultPayload.Length - 4));
                var eq = Assert.ThrowsException<InvalidDataException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                Assert.AreEqual($"Invalid value time const. Expected {Argon2ID.DefaultTimeCost} real {Argon2ID.DefaultTimeCost + 1}", eq.Message);
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                ms_custom_default_value.Write(defaultPayload.AsSpan(0, 4));
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, Argon2ID.DefaultMemorySize + 1);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(8, defaultPayload.Length - 8));
                var eq = Assert.ThrowsException<InvalidDataException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                Assert.AreEqual($"Invalid value memory size const. Expected {Argon2ID.DefaultMemorySize} real {Argon2ID.DefaultMemorySize + 1}", eq.Message);
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                MemoryStream ms_custom_default_value = new MemoryStream();
                ms_custom_default_value.Write(defaultPayload.AsSpan(0, 8));
                Span<byte> buf_val = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(buf_val, Argon2ID.DefaultParallelism + 1);
                ms_custom_default_value.Write(buf_val);
                ms_custom_default_value.Write(defaultPayload.AsSpan(12, defaultPayload.Length - 12));
                var eq = Assert.ThrowsException<InvalidDataException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, ms_custom_default_value.ToArray()); });
                Assert.AreEqual($"Invalid value parallelism const. Expected {Argon2ID.DefaultParallelism} real {Argon2ID.DefaultParallelism + 1}", eq.Message);
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }

            {
                byte[] pass = GetDefaultPassword();
                var eq1 = Assert.ThrowsException<InvalidDataException>(() => { var kdf = new Argon2ID(ref pass, KdfType.Argon2id, buffer); });
                Assert.AreEqual("Invalid size payload", eq1.Message);
                CollectionAssert.AreEqual(Array.Empty<byte>(), pass);
            }
        }
        [TestMethod]
        public void Argon2ID_ConsistentTest()
        {
            byte[] local_pass1 = GetDefaultPassword();
            byte[] local_pass2 = GetDefaultPassword();
            using var kdf1 = new Argon2ID(ref local_pass1, Core.Crypto.KdfType.Argon2id);
            Assert.AreEqual(local_pass1, Array.Empty<byte>());
            using var kdf2 = new Argon2ID(ref local_pass2, Core.Crypto.KdfType.Argon2id);
            Assert.AreEqual(local_pass2, Array.Empty<byte>());
            CollectionAssert.AreNotEqual(kdf1.Key.ToArray(), kdf2.Key.ToArray());
            using MemoryStream test_ms = new MemoryStream();
            kdf2.MoveTo(test_ms);
            byte[] salt = new byte[32];
            byte[] buffer = test_ms.ToArray();
            buffer.AsSpan(SizePreDataPayload + kdf2.SizePayload - salt.Length, salt.Length).CopyTo(salt);
            
            Konscious.Security.Cryptography.Argon2id a2id = new Argon2id(GetDefaultPassword()) { 
                MemorySize = Argon2ID.DefaultMemorySize,
                DegreeOfParallelism = Argon2ID.DefaultParallelism,
                Iterations = Argon2ID.DefaultTimeCost,
                Salt = salt
            
            };
            byte[] key_imp = a2id.GetBytes(32);
            CollectionAssert.AreEqual(key_imp, kdf2.Key.ToArray());
        }
        [TestMethod]
        public void Argon2ID_Dispose()
        {
            byte[] pass = GetDefaultPassword();
            var kdf = new Argon2ID(ref pass, KdfType.Argon2id);
            Span<byte> key = kdf.Key;
            Assert.AreEqual(pass, Array.Empty<byte>());
            kdf.Dispose();
            byte[] zero_byte = new byte[kdf.Key.Length];
            Array.Fill<byte>(zero_byte, 0);
            CollectionAssert.AreEqual(zero_byte, key.ToArray());
        }
        [TestMethod]
        public void Argon2ID_SerializationRoundTrip_ProducesSameKey()
        {
            byte[] first_key;
            byte[] second_key;
            uint defaultSizePayload;

            MemoryStream ms = new MemoryStream();
            
            {
                byte[] pass = GetDefaultPassword();
                var kdf = new Argon2ID(ref pass, KdfType.Argon2id);
                first_key = kdf.Key.ToArray();
                Assert.AreEqual(pass, Array.Empty<byte>());
                kdf.MoveTo(ms);
                defaultSizePayload = (uint)kdf.SizePayload;
            }

            byte[] serialized = ms.ToArray();

            int offset = 0;

            byte kdfId = serialized[offset];
            offset += 1;
            Assert.AreEqual((byte)KdfType.Argon2id, kdfId);

            uint payloadLen = BinaryPrimitives.ReadUInt32LittleEndian(
                serialized.AsSpan(offset, 4));
            offset += 4;

            Assert.AreEqual(payloadLen, defaultSizePayload);

            byte[] payload = serialized .AsSpan(offset, checked((int)payloadLen)).ToArray();

            {
                byte[] pass = GetDefaultPassword();
                using var kdf = new Argon2ID(ref pass, KdfType.Argon2id, payload);
                second_key = kdf.Key.ToArray();
            }

            CollectionAssert.AreEqual(first_key, second_key);
        }
    }
}
