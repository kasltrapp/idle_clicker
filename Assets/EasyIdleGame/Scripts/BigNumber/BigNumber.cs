using System;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Struct that holds a big number and provides methods to work with it
    /// </summary>
    [Serializable]
    public partial struct BigNumber : IComparable<BigNumber>
    {
        public static BigNumber Zero => new BigNumber(0, 0);
        public static BigNumber One => new BigNumber(1, 0);

        // letters is used for exponents 3, 6, 9, 12, etc. (eg 1,000 is 1K, 1,000,000 is 1M)
        public static char[] exponentLetters = { 'K', 'M', 'B', 'T' };
        // letters is used for exponents are used after the letters array is exhausted and it goes like this: AA, AB, AC ... BA, BB, BC ... ZA, ZB, ZC
        public static char[] ndRoundLetters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        [Tooltip("Significant digits of the value. BigNumber normalizes this to roughly the 1-10 range while Exponent stores the scale.")]
        public double Mantissa;

        [Tooltip("Base-10 exponent applied to Mantissa. For example, Mantissa 1.5 and Exponent 6 represents 1,500,000.")]
        public long Exponent;

        // number this low makes the numbers much more stable
        public const int ROUNDING = 7;
        public const double COMPARISON_VALUE = 1e-7;

        public const int DISPLAY_ROUNDING = 2;

        public static BigNumber MinPositiveValue => new(1, -ROUNDING);

        public BigNumber(double mantissa = 1, long exponent = 0)
        {
            // just to initialize the struct
            Mantissa = Exponent = 0;

            Normalize(mantissa, exponent);
        }

        public BigNumber Round(int rouding = ROUNDING)
        {
            if (Exponent < 0) rouding = (int)Math.Min(rouding + 3, rouding + (Exponent));

            double mantissa = Math.Round(Mantissa, rouding);

            return new BigNumber(mantissa, Exponent);
        }

        public BigNumber RoundToWholeNumber() => RoundToWholeNumber(out _);

        // Rounds the number to the nearest whole number
        public BigNumber RoundToWholeNumber(out int roundedTo)
        {
            roundedTo = 0;

            if (Exponent < 0) return new BigNumber(0);

            roundedTo = Math.Max(Math.Min((int)Exponent, 15), 0);

            double mantissa = Math.Round(Mantissa, roundedTo);

            return new BigNumber(mantissa, Exponent);
        }

        public BigNumber Floor()
        {
            BigNumber rounded = RoundToWholeNumber(out int roundedTo);

            if (rounded > new BigNumber(Mantissa, Exponent))
            {
                if (rounded.Mantissa == 1)
                {
                    // patching at its finest, it passes all my tests, but damn
                    rounded = new BigNumber(10, rounded.Exponent - 1);
                    roundedTo++;
                }

                double rNum = 1d / Math.Pow(10, roundedTo);

                return new BigNumber(rounded.Mantissa - rNum, rounded.Exponent);
            }

            return rounded;
        }

        public BigNumber Ceil()
        {
            BigNumber rounded = RoundToWholeNumber(out int roundedTo);

            if (rounded < new BigNumber(Mantissa, Exponent))
            {
                double rNum = 1d / Math.Pow(10, roundedTo);
                return new BigNumber(rounded.Mantissa + rNum, rounded.Exponent);
            }

            return rounded;
        }

        public void Normalize(int rouding = ROUNDING) => Normalize(Mantissa, Exponent, rouding);

        // Normalize the number to keep the mantissa in a manageable range
        public void Normalize(double mantissa, long exponent, int rouding = ROUNDING)
        {
            if (mantissa == 0)
            {
                Mantissa = Exponent = 0;
                return;
            }

            while (Math.Abs(mantissa) >= 10)
            {
                mantissa /= 10;
                exponent++;
            }

            while (Math.Abs(mantissa) < 1 && mantissa != 0)
            {
                mantissa *= 10;
                exponent--;
            }

            mantissa = Math.Round(mantissa, rouding + 1);

            Mantissa = mantissa;
            Exponent = exponent;
        }

        public BigNumber Add(BigNumber other)
        {
            if (Exponent == other.Exponent)
                return new BigNumber(Mantissa + other.Mantissa, Exponent);

            BigNumber bigger, smaller;
            if (Exponent > other.Exponent)
            {
                bigger = this;
                smaller = other;
            }
            else
            {
                bigger = other;
                smaller = this;
            }

            double adjustedMantissa = smaller.Mantissa / Math.Pow(10, bigger.Exponent - smaller.Exponent);

            return new BigNumber(bigger.Mantissa + adjustedMantissa, bigger.Exponent);
        }

        public BigNumber Subtract(BigNumber other)
            => Add(new BigNumber(-other.Mantissa, other.Exponent));

        public BigNumber Multiply(BigNumber other)
            => new BigNumber(Mantissa * other.Mantissa, Exponent + other.Exponent);

        public BigNumber Divide(BigNumber other)
        {
            if (other.Mantissa == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            return new BigNumber(Mantissa / other.Mantissa, Exponent - other.Exponent);
        }

        public BigNumber Modulo(BigNumber other)
        {
            if (other.Mantissa == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            // If exponents are equal, directly apply modulo to mantissas
            if (Exponent == other.Exponent)
            {
                return new BigNumber(Mantissa % other.Mantissa, Exponent);
            }

            // Adjust the larger exponent to match the smaller one
            if (Exponent > other.Exponent)
            {
                double scaledMantissa = Mantissa * Math.Pow(10, Exponent - other.Exponent);
                double resultMantissa = scaledMantissa % other.Mantissa;
                return new BigNumber(resultMantissa, other.Exponent);
            }
            else
            {
                double scaledOtherMantissa = other.Mantissa * Math.Pow(10, other.Exponent - Exponent);
                double resultMantissa = Mantissa % scaledOtherMantissa;
                return new BigNumber(resultMantissa, Exponent);
            }
        }

        // Convert the exponent to a letter-based format (e.g., A, B, ..., AA, AB, etc.)
        private static string ExponentToLetters(long exponent)
        {
            if (exponent < 3) return string.Empty;

            long adjustedExponent = (exponent - 3) / 3;

            if (adjustedExponent < exponentLetters.Length)
                return exponentLetters[adjustedExponent].ToString();

            adjustedExponent -= exponentLetters.Length;

            // so there is one more exponent for these letters
            adjustedExponent += ndRoundLetters.Length;

            string result = string.Empty;

            while (adjustedExponent >= 0)
            {
                result = ndRoundLetters[adjustedExponent % ndRoundLetters.Length] + result;
                adjustedExponent = adjustedExponent / ndRoundLetters.Length - 1;
            }

            return result;
        }

        private string MantissaToString(double Mantissa, long exponent)
        {
            long adjustedExponent = exponent % 3;

            double num = Mantissa * Math.Pow(10, adjustedExponent);

            if (adjustedExponent == 2 || exponent < 3) return $"{num:F0}";
            if (adjustedExponent == 1) return $"{num:F1}";
            return $"{num:F2}";
        }

        private string MantissaToString_CustomDec(double Mantissa, long exponent, int decimalPoints)
        {
            long adjustedExponent = exponent % 3;

            double num = Mantissa * Math.Pow(10, adjustedExponent);

            return $"{num.ToString($"F{decimalPoints}")}";
        }

        public override string ToString() => ToString(false);

        public string ToString(bool showDecimal)
        {
            if (Mantissa == 0) return "0";

            if (Exponent < 2)
            {
                if (Exponent < -2) return "0";

                double num = ToDouble();

                if (!showDecimal) return num.ToString("F0");
                if (num == ToInt()) return num.ToString();
                switch (Exponent)
                {
                    case -1:
                        return num.ToString("F1");
                    case 0:
                        return num.ToString("F2");
                    case 1:
                        return num.ToString("F1");
                }
            }

            return $"{MantissaToString(Mantissa, Exponent)}{ExponentToLetters(Exponent)}";
        }

        public string ToString(int decimalPoints)
        {
            if (decimalPoints < 0)
                throw new ArgumentException("Decimal points cannot be negative", nameof(decimalPoints));
            if (decimalPoints > 15)
                throw new ArgumentException("Decimal points cannot be greater than 15", nameof(decimalPoints));

            return $"{MantissaToString_CustomDec(Mantissa, Exponent, decimalPoints)}{ExponentToLetters(Exponent)}";
        }

        public string ToStringDebug() => $"{Mantissa}e{Exponent}";

        public int CompareTo(BigNumber other)
        {
            Normalize(Mantissa, Exponent);
            other.Normalize(other.Mantissa, other.Exponent);

            if (other.Mantissa == 0 && Mantissa != 0) return Mantissa.CompareTo(other.Mantissa);
            if (Mantissa == 0 && other.Mantissa != 0) return Mantissa.CompareTo(other.Mantissa);

            if (other.Mantissa < 0 && Mantissa > 0) return 1;
            else if (other.Mantissa > 0 && Mantissa < 0) return -1;

            if (Exponent == other.Exponent)
                return Mantissa.CompareTo(other.Mantissa);

            if (Mantissa > 0)
                return Exponent.CompareTo(other.Exponent);
            else
                return other.Exponent.CompareTo(Exponent);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            BigNumber other = (BigNumber)obj;

            return Math.Abs(Mantissa - other.Mantissa) < COMPARISON_VALUE && Exponent == other.Exponent;
        }

        public override readonly int GetHashCode() => Mantissa.GetHashCode() ^ Exponent.GetHashCode();

        public static BigNumber MaxValue => new BigNumber(9, long.MaxValue);
        public static BigNumber MinValue => new BigNumber(-9, long.MaxValue);

        public readonly double ToDouble() => Mantissa * Math.Pow(10, Exponent);
        public readonly int ToInt() => (int)ToDouble();
        public readonly long ToLong() => (long)ToDouble();

        public BigNumber Pow(BigNumber power)
        {
            if (power < 0)
            {
                return new BigNumber(Math.Pow(Mantissa, power.ToInt()), Exponent * power.ToInt());
            }

            if (power > double.MaxValue)
                throw new ArgumentException($"Power is too large. ({power.ToStringDebug()} > {new BigNumber(long.MaxValue).ToStringDebug()})");

            long exp = power.ToLong(); // safe since power is small enough
            BigNumber result = new BigNumber(1, 0); // represents 1

            BigNumber baseNumber = this;

            while (exp > 0)
            {
                if ((exp & 1) == 1)
                    result = result.Multiply(baseNumber);

                baseNumber = baseNumber.Multiply(baseNumber);
                exp >>= 1;
            }

            return result;
        }
    }
}
