using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Crypto.Constraints
{
    public static class Totp
    {
        public const int MinDigits = 4;
        public const int MaxDigits = 8;

        public const int MinPeriodSeconds = 15;
        public const int MaxPeriodSeconds = 120;

        public static void Validate(int digits, ulong period)
        {
            if (digits < MinDigits || digits > MaxDigits)
                throw new ArgumentOutOfRangeException(nameof(digits),
                    $"TOTP digits must be in range [{MinDigits};{MaxDigits}]");

            if (period < MinPeriodSeconds || period > MaxPeriodSeconds)
                throw new ArgumentOutOfRangeException(nameof(period),
                    $"TOTP period must be in range [{MinPeriodSeconds};{MaxPeriodSeconds}] seconds");
        }
    }
}
