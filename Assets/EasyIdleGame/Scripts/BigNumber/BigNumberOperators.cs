
namespace EasyIdleGame
{
    /// <summary>
    /// Class that overloads operators for BigNumber
    /// </summary>
    public partial struct BigNumber
    {
        public static BigNumber operator +(BigNumber a, BigNumber b) => a.Add(b);

        public static BigNumber operator -(BigNumber a, BigNumber b) => a.Subtract(b);

        public static BigNumber operator *(BigNumber a, BigNumber b) => a.Multiply(b);

        public static BigNumber operator /(BigNumber a, BigNumber b) => a.Divide(b);

        public static BigNumber operator %(BigNumber a, BigNumber b) => a.Modulo(b);

        public static bool operator ==(BigNumber a, BigNumber b) => a.Equals(b);

        public static bool operator !=(BigNumber a, BigNumber b) => !a.Equals(b);

        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;

        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;

        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;

        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;

        public static BigNumber operator ++(BigNumber a) => a.Add(new BigNumber(1));

        public static BigNumber operator --(BigNumber a) => a.Subtract(new BigNumber(1));

        public static BigNumber operator -(BigNumber v)
        {
            return new BigNumber(-v.Mantissa, v.Exponent);
        }

        public static implicit operator BigNumber(int value) => new BigNumber(value);

        public static implicit operator BigNumber(float value) => new BigNumber(value);

        public static implicit operator BigNumber(double value) => new BigNumber(value);

        public static implicit operator BigNumber(long value) => new BigNumber(value);

        public static implicit operator BigNumber(decimal value) => new BigNumber((double)value);

        public static BigNumber Min(BigNumber a, BigNumber b) => a < b ? a : b;

        public static BigNumber Max(BigNumber a, BigNumber b) => a > b ? a : b;
    }
}