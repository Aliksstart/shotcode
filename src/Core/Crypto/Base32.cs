namespace Core.Crypto
{
    public static class Base32
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static readonly int[] ReverseAlphabet = CreateReverseLookup();
        private static int[] CreateReverseLookup()
        {
            var reverse = new int[128];
            Array.Fill(reverse, -1);
            for (int i = 0; i < Alphabet.Length; i++)
            {
                reverse[Alphabet[i]] = i;
            }
            return reverse;
        }

        private static int GetSignificantLength(ReadOnlySpan<char> src)
        {
            if (src.Length % 8 != 0)
                throw new InvalidDataException($"Invalid size {nameof(src)}");
            int end = 0;
            for (int i = src.Length - 1; i >= 0; --i)
            {
                if (src[i] != '=')
                {
                    end = 1 + i;
                    break;
                }
            }
            int pad = src.Length - end;
            if (pad != 0 && pad != 1 && pad != 3 && pad != 4 && pad != 6)
                throw new InvalidDataException("Inaccessible padding.");
            return end;
        }

        public static void Decode(ReadOnlySpan<char> src, Span<byte> dst)
        {
            if (src.Length == 0)
            {
                if (dst.Length != 0)
                    throw new InvalidDataException($"{nameof(dst)} invalid size");
                return;
            }
            int end = GetSignificantLength(src);
            if (dst.Length != end * 5 / 8)
                throw new InvalidDataException($"{nameof(dst)} size invalid.");
            int curr = 0;
            dst.Clear();
            while (curr < end * 5)
            {
                int el = curr / 5;
                int index = curr / 8;
                int offset = curr % 8;
                int i = ReverseAlphabet[src[el]];
                if (i == -1)
                    throw new InvalidDataException($"The symbol '{src[el]}' was not found in the alphabet.");
                if (8 - (5 + offset) >= 0)
                {
                    dst[index] |= (byte)(i << (8 - (5 + offset)));
                }
                else
                {
                    int k = ((-1) * (8 - (5 + offset)));
                    dst[index] |= (byte)(i >> k);
                    if (index + 1 < dst.Length)
                    {
                        dst[index + 1] |= (byte)(i << (8 - k));
                    }
                    else if ((i & (1 << k) - 1) != 0)
                    {
                        throw new InvalidDataException("Non-zero padding bits in the last symbol.");
                    }
                }

                curr += 5;
            }
        }

        public static void Encode(ReadOnlySpan<byte> src, Span<char> dst)
        {
            if (src.Length == 0)
            {
                if (dst.Length != 0)
                    throw new InvalidDataException($"{nameof(dst)} invalid size");
                return;
            }
            int required_length = GetEncodeLength(src.Length);
            if (dst.Length != required_length)
                throw new InvalidDataException($"{nameof(dst)} size invalid.");
            dst.Fill('=');
            int totalBits = src.Length * 8;
            int currStart = 0;
            int res_el = 0;
            while (currStart < totalBits)
            {
                int index = currStart / 8;
                int offset = currStart % 8;
                int value;

                if (offset <= 3)
                {
                    value = (src[index] >> (3 - offset)) & 0x1F;
                }
                else
                {
                    int hi = (src[index] << (offset - 3)) & 0x1F;
                    int lo = index + 1 < src.Length ? src[index + 1] >> (11 - offset) : 0;
                    value = (hi | lo) & 0x1F;
                }
                dst[res_el] = Alphabet[value];
                res_el++;
                currStart += 5;
            }

        }

        public static int GetEncodeLength(int byteCount)
        {
            return ((byteCount + 4) / 5) * 8;
        }
        public static int GetDecodeLength(ReadOnlySpan<char> src) => src.Length == 0 ? 0 : GetSignificantLength(src) * 5 / 8;
    }
}
