using Core.Crypto;
using System.Buffers.Binary;
using System.Data;

namespace Core
{
    public class ValutOrigin : Valut
    {
        KDF _kdf;
        private static bool TryGetKdfPayloadSize(KdfType type, out int size)
        {
            switch (type)
            {
                case KdfType.Argon2id:
                    size = Argon2ID.PayloadSize;
                    return true;

                default:
                    size = 0;
                    return false;
            }
        }
        public ValutOrigin(SCDB db, ref byte[] password, int interval, Action lockedEvent) : base(db, interval, lockedEvent, ValutState.Locked)
        {
            (byte[] nonce, byte[] tag, byte[] origin) = db.GetOriginBlock();
            if (origin.Length < 5) //4 + 4 + 32)
                throw new Exception("Origin block too small");
            byte kdf_id;
            uint raw_kdf_len;
            //byte[] kdf_payload;
            int offset = 0;
            kdf_id = origin[0];
            offset += 1;
            KdfType kdf_type = (KdfType)kdf_id;
            raw_kdf_len = BinaryPrimitives.ReadUInt32LittleEndian(
                origin.AsSpan(offset, 4));
            offset += 4;
            if (!TryGetKdfPayloadSize(kdf_type, out int expected_kdf_len))
                throw new FormatException("Unsupported KDF type.");
            if (raw_kdf_len != expected_kdf_len)
                throw new FormatException("Invalid KDF payload size.");
            if (origin.Length < offset + expected_kdf_len)
                throw new FormatException("Invalid KDF payload.");
            //ReadOnlySpan<byte> kdf_payload = origin.AsSpan(offset, expected_kdf_len);
            byte[] kdf_payload = new byte[expected_kdf_len];
            origin.AsSpan(offset, expected_kdf_len).CopyTo(kdf_payload);
            offset += expected_kdf_len;
            
            switch (kdf_type)
            {
                case KdfType.Argon2id:
                    _kdf = new Argon2ID(ref password, kdf_type, kdf_payload.ToArray());
                    break;
                default:
                    throw new FormatException("Unsupported KDF format");
            }
            int text_size = origin.Length - offset;
            if (text_size < 1)
                throw new FormatException("Invalid size ciphertext");
            byte[] ciphertext = new byte[text_size];
            origin.AsSpan(offset, text_size).CopyTo(ciphertext);
            base.Open(_kdf.Key, nonce, tag, ciphertext);
        }

        public ValutOrigin(SCDB db, ref byte[] password, KdfType kdfType, int interval, Action lockedEvent) : base(db, interval, lockedEvent, ValutState.Dirty)
        {
            if (!db.IsClear())
                throw new FormatException("It is not possible to create ValutOrigin in a non-empty database.");
            switch (kdfType) { 
                case KdfType.Argon2id:
                    _kdf = new Argon2ID(ref password, kdfType);
                    break;
                default: throw new FormatException("Unsupported KDF format");
            }
        }

        public override void Save()
        {
            MemoryStream ms = new MemoryStream();
            _kdf.MoveTo(ms);
            byte[] nonce, gcm_tag;
            base.EncSerializePayload(ms, _kdf.Key, out nonce, out gcm_tag);
            base._db.setCryptoOrigin(nonce, gcm_tag, ms.ToArray());
            base.Save();
        }
        public string[] GetNameServices() {
            return base.Services;
        }
        new public bool AddBlock(Block block)
        {
            return base.AddBlock(block);
        }
        new public bool RemoveBlock(string name)
        {
            return base.RemoveBlock(name);
        }
        new public void Close() 
        {
            base.Close();
        }

        public override void Dispose()
        {
            _kdf?.Dispose();
            base.Dispose();
        }
    }
}
