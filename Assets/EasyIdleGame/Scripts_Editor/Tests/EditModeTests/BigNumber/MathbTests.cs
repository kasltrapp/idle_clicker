using NUnit.Framework;

namespace EasyIdleGame.Tests
{
    public static class MathbTests
    {
        [Test]
        public static void Log()
        {
            Assert.IsTrue(Mathb.Log(1, 10) == 0, "Log(1, 10) should be 0");
            Assert.IsTrue(Mathb.Log(10, 10) == 1, "Log(10, 10) should be 1");

            Assert.IsTrue(Mathb.Log(100, 10) == 2, "Log(100, 10) should be 2");
            Assert.IsTrue(Mathb.Log(1000, 10) == 3, "Log(1000, 10) should be 3");
            Assert.IsTrue(Mathb.Log(10000, 10) == 4, "Log(10000, 10) should be 4");
            Assert.IsTrue(Mathb.Log(100000, 10) == 5, "Log(100000, 10) should be 5");

            Assert.IsTrue(Mathb.Log(1, 2) == 0, "Log(1, 2) should be 0");
            Assert.IsTrue(Mathb.Log(2, 2) == 1, "Log(2, 2) should be 1");
            Assert.IsTrue(Mathb.Log(4, 2) == 2, "Log(4, 2) should be 2");
            Assert.IsTrue(Mathb.Log(8, 2) == 3, "Log(8, 2) should be 3");
            Assert.IsTrue(Mathb.Log(16, 2) == 4, "Log(16, 2) should be 4");

            Assert.IsTrue(Mathb.Log(1, 3) == 0, "Log(1, 3) should be 0");
            Assert.IsTrue(Mathb.Log(3, 3) == 1, "Log(3, 3) should be 1");
            Assert.IsTrue(Mathb.Log(9, 3) == 2, "Log(9, 3) should be 2");
        }

        [Test]
        public static void GeometricSequence_Sum()
        {
            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2, 3) == 7, "GeometricSequence_Sum(1, 2, 3) should be 7");
            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2, 4) == 15, "GeometricSequence_Sum(1, 2, 4) should be 15");
            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2, 5) == 31, "GeometricSequence_Sum(1, 2, 5) should be 31");

            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2.25, 1) == 1, "GeometricSequence_Sum(1, 2.25, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2.25, 2) == 3.25, "GeometricSequence_Sum(1, 2.25, 2) should be 3.25");

            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 1, 1) == 1, "GeometricSequence_Sum(1, 1, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_Sum(1, 2, 1) == 1, "GeometricSequence_Sum(1, 2, 1) should be 1");
        }

        [Test]
        public static void GeometricSequence_NthTerm()
        {
            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2, 3) == 4, "GeometricSequence_NthTerm(1, 2, 3) should be 4");
            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2, 4) == 8, "GeometricSequence_NthTerm(1, 2, 4) should be 8");
            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2, 5) == 16, "GeometricSequence_NthTerm(1, 2, 5) should be 16");

            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2.25, 1) == 1, "GeometricSequence_NthTerm(1, 2.25, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2.25, 2) == 2.25, "GeometricSequence_NthTerm(1, 2.25, 2) should be 2.25");

            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 1, 1) == 1, "GeometricSequence_NthTerm(1, 1, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_NthTerm(1, 2, 1) == 1, "GeometricSequence_NthTerm(1, 2, 1) should be 1");
        }

        [Test]
        public static void GeometricSequence_Amount()
        {
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2, 7) == 3, "GeometricSequence_Amount(1, 2, 7) should be 3");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2, 15) == 4, "GeometricSequence_Amount(1, 2, 15) should be 4");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2, 31) == 5, "GeometricSequence_Amount(1, 2, 31) should be 5");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2.25, 1) == 1, "GeometricSequence_Amount(1, 2.25, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2.25, 3.25) == 2, "GeometricSequence_Amount(1, 2.25, 3.25) should be 2");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 1, 1) == 1, "GeometricSequence_Amount(1, 1, 1) should be 1");
            Assert.IsTrue(Mathb.GeometricSequence_Amount(1, 2, 1) == 1, "GeometricSequence_Amount(1, 2, 1) should be 1");
        }

        [Test]
        public static void RandomBetween()
        {
            Assert.IsTrue(Mathb.RandomBetween(0, 0) == 0, "RandomBetween(0, 0) should be 0");
            Assert.IsTrue(Mathb.RandomBetween(5, 5) == 5, "RandomBetween(5, 5) should be 5");

            Assert.Throws<System.ArgumentException>(() => Mathb.RandomBetween(10, 5), "RandomBetween(10, 5) should throw ArgumentException");

            BigNumber result1 = Mathb.RandomBetween(1, 10);
            Assert.IsTrue(result1 >= 1 && result1 <= 10, $"RandomBetween(1, 10) returned {result1.ToStringDebug()}, which should be between 1 and 10");

            BigNumber result2 = Mathb.RandomBetween(new BigNumber(1, 6), new BigNumber(5, 6));
            Assert.IsTrue(result2 >= new BigNumber(1, 6) && result2 <= new BigNumber(5, 6), "RandomBetween with large BigNumbers should be within range");
        }

        [Test]
        public static void GeometricSequence_InverseTests_SumToAmount()
        {
            InverseTest_SumToAmount(1, 2, 3, "Sum to amount inverse test for 1, 2, 3");
            InverseTest_SumToAmount(1, 4, 100, "Sum to amount inverse test for 1, 4, 100");
            InverseTest_SumToAmount(1, 2, 2_500, "Sum to amount inverse test for 1, 2, 2500");
            InverseTest_SumToAmount(1, 20, 2_500_000, "Sum to amount inverse test for 1, 20, 2500000");
            InverseTest_SumToAmount(1_000, 20_000, 2_500_000_000, "Sum to amount inverse test for large constants");
            InverseTest_SumToAmount(123_456, 123_456_789, 123_456_789_123, "Sum to amount inverse test for specific integers");
            InverseTest_SumToAmount(new BigNumber(2.25, 5), new BigNumber(2.25, 5), new BigNumber(2.25, 17), "Sum to amount inverse test for BigNumbers");
        }

        public static void InverseTest_SumToAmount(BigNumber a, BigNumber q, BigNumber n, string message)
        {
            BigNumber sum = Mathb.GeometricSequence_Sum(a, q, n);
            BigNumber amount = Mathb.GeometricSequence_Amount(a, q, sum);

            Assert.IsTrue(amount == n, $"{message} - Expected {n}, got {amount}");
        }
    }
}