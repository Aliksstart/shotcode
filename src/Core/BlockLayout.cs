namespace Core
{
    public static class BlockLayout
    {
        public const int VersionSize = 1;
        public const int TypeSize = 1;
        public const int DigitsSize = 1;
        public const int AlgorithmSize = 1;
        public const int TimestampSize = 8;
        public const int PeriodOrCounterSize = 8;
        public const int LengthSize = 4;

        public const int VersionOffset = 0;
        public const int TypeOffset = VersionOffset + VersionSize;
        public const int DigitsOffset = TypeOffset + TypeSize;
        public const int AlgorithmOffset = DigitsOffset + DigitsSize;
        public const int CreatedTSOffset = AlgorithmOffset + AlgorithmSize;
        public const int UpdatedTSOffset = CreatedTSOffset + TimestampSize;
        public const int PeriodOrCounterOffset = UpdatedTSOffset + TimestampSize;
        public const int ServiceNameLengthOffset = PeriodOrCounterOffset + PeriodOrCounterSize;
        public const int SecretLengthOffset = ServiceNameLengthOffset + LengthSize;
        public const int ExtraLengthOffset = SecretLengthOffset + LengthSize;

        public const int DataOffset = ExtraLengthOffset + LengthSize;
        public const int MinBlockSize = DataOffset;
        public const int ServiceNameOffset = DataOffset;
    }
}
