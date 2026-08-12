using System;

namespace EasyIdleGame
{
    /// <summary>
    /// <para><b>Geometric Sequence Legend</b></para>
    /// 
    /// <para> 'a' (a1) - first term </para>
    /// <para> 'n' - element count </para>
    /// <para> 'q' - geometric sequence quotient </para>
    /// 
    /// </summary>
    public static class Mathb
    {
        public static BigNumber GeometricSequence_Sum(BigNumber a, BigNumber q, BigNumber n)
        {
            if (q == 1) return a * n;

            return a * (1 - q.Pow(n)) / (1 - q);
        }

        public static BigNumber GeometricSequence_NthTerm(BigNumber a, BigNumber q, BigNumber n)
        {
            return a * q.Pow(n - 1);
        }

        public static BigNumber Log(BigNumber a, BigNumber b)
        {
            if (a.Mantissa <= 0 || b.Mantissa <= 0)
                throw new ArgumentException($"Logarithm undefined for non-positive numbers. (a: {a.ToStringDebug()}, b: {b.ToStringDebug()})");

            double log10Value = Math.Log10(a.Mantissa) + a.Exponent;
            double log10Base = Math.Log10(b.Mantissa) + b.Exponent;

            if (log10Base == 0 || log10Value == 0) return 0;

            double result = log10Value / log10Base;

            long resultExponent = (long)Math.Floor(Math.Log10(Math.Abs(result)));
            double resultMantissa = result / Math.Pow(10, resultExponent);

            return new BigNumber(resultMantissa, resultExponent);
        }

        // float version
        public static float GeometricSequence_NthTerm_Float(float a, float q, int n)
        {
            return (float)(a * Math.Pow(q, n - 1));
        }

        // experimental
        /// <returns> The amount of terms in a geometric sequence given the first term, the quotient, and the sum. </returns>
        public static BigNumber GeometricSequence_Amount(BigNumber a, BigNumber q, BigNumber sum)
        {
            if (a == 0) return 0;

            if (q == 1) return sum / a;

            BigNumber x = (q - 1) * sum;

            return Log(x / a + 1, q);
        }

        public static BigNumber RandomBetween(BigNumber min, BigNumber max)
        {
            if (min == max) return min;
            if (min > max) throw new ArgumentException("Min cannot be greater than Max");

            BigNumber diff = max - min;
            double randomVal = UnityEngine.Random.value;
            return min + diff * new BigNumber(randomVal, 0);
        }
    }
}
