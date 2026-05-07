using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class SCDBLayout
    {
        // размеры полей
        public const int MagicSize = 4;
        public const int VersionSize = 4;
        public const int TimestampSize = 8; // created, updated, tpm_updated
        public const int NonceSize = 12;
        public const int ReservedSize = 16;
        public const int GcmTagSize = 16;
        public const int ValueLenghtSize = 4;

        // вычисляемые оффсеты
        public static int VersionOffset => MagicSize;
        public static int UpdateTSOffset => VersionOffset + VersionSize + TimestampSize;
        public static int OriginNonceOffset => UpdateTSOffset + TimestampSize + TimestampSize;
        public static int OriginLenOffset => OriginNonceOffset + NonceSize + NonceSize + ReservedSize;
        public static int OriginTagOffset => OriginLenOffset + ValueLenghtSize + ValueLenghtSize;
        public static int OriginCiphertextOffset => OriginTagOffset + GcmTagSize + GcmTagSize; // + len_crypto_origin + len_crypto_tpm
        

        public static long MinFileSize =>
    MagicSize + VersionSize + 3 * TimestampSize +
    NonceSize + NonceSize + ReservedSize +
    ValueLenghtSize + ValueLenghtSize +  // len_crypto_origin + len_crypto_tpm
    GcmTagSize + GcmTagSize;
    }
}
