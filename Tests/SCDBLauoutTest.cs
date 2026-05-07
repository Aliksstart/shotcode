using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SCDBLauoutTest
    {
        private int _magicSize = 4;
        private int _version_size = 4;
        private int _created_ts_size = 8;
        private int _updated_ts_size = 8;
        private int _tpm_updated_ts_size = 8; // not used before version 2.0 and later
        private int _nonce_origin_size = 12;
        private int _nonce_tpm_size = 12;
        private int _reserved_size = 16;
        private int _len_crypto_origin_size = 4;
        private int _len_crypto_tpm_size = 4;
        private int _origin_gcm_tag_size = 16;
        private int _tpm_gcm_tag_size = 16;

        [TestMethod]
        public void LayoutSizes_ShouldMatchActualFields()
        {
            // проверяем размеры массивов
            Assert.AreEqual(SCDBLayout.MagicSize, _magicSize);
            Assert.AreEqual(SCDBLayout.VersionSize, _version_size);
            Assert.AreEqual(SCDBLayout.TimestampSize, _created_ts_size);
            Assert.AreEqual(SCDBLayout.TimestampSize, _updated_ts_size);
            Assert.AreEqual(SCDBLayout.TimestampSize, _tpm_updated_ts_size);
            Assert.AreEqual(SCDBLayout.NonceSize, _nonce_origin_size);
            Assert.AreEqual(SCDBLayout.NonceSize, _nonce_tpm_size);
            Assert.AreEqual(SCDBLayout.ReservedSize, _reserved_size);
            Assert.AreEqual(SCDBLayout.ValueLenghtSize, _len_crypto_origin_size);
            Assert.AreEqual(SCDBLayout.ValueLenghtSize, _len_crypto_tpm_size);
            Assert.AreEqual(SCDBLayout.GcmTagSize, _origin_gcm_tag_size);
            Assert.AreEqual(SCDBLayout.GcmTagSize, _tpm_gcm_tag_size);
            int header_size = _magicSize + _version_size + _created_ts_size +
                _updated_ts_size + _tpm_updated_ts_size + _nonce_origin_size +
                _nonce_tpm_size + _reserved_size + _len_crypto_origin_size +
                _len_crypto_tpm_size + _origin_gcm_tag_size + _tpm_gcm_tag_size;
            Assert.AreEqual(SCDBLayout.MinFileSize, header_size);
        }
        [TestMethod]
        public void LayoutSizes_ShouldMatchOffsets() {
            int offset_to_version = _magicSize;
            Assert.AreEqual(SCDBLayout.VersionOffset, offset_to_version);
            int offset_to_updated = offset_to_version + _version_size + _created_ts_size;
            Assert.AreEqual(SCDBLayout.UpdateTSOffset, offset_to_updated);
            int offset_to_origin_nonce = offset_to_updated + _updated_ts_size + _tpm_updated_ts_size;
            Assert.AreEqual(SCDBLayout.OriginNonceOffset, offset_to_origin_nonce);
            int offset_to_len_crypto_origin = offset_to_origin_nonce + _nonce_origin_size + _nonce_tpm_size + _reserved_size;
            Assert.AreEqual(SCDBLayout.OriginLenOffset, offset_to_len_crypto_origin);
            int offset_to_original_gcm_tag = offset_to_len_crypto_origin + _len_crypto_origin_size + _len_crypto_tpm_size;
            Assert.AreEqual(SCDBLayout.OriginTagOffset, offset_to_original_gcm_tag);
            int offset_to_origin_crypto_text = offset_to_original_gcm_tag + _origin_gcm_tag_size + _tpm_gcm_tag_size;
            Assert.AreEqual(SCDBLayout.OriginCiphertextOffset, offset_to_origin_crypto_text);
        }
    }
}
