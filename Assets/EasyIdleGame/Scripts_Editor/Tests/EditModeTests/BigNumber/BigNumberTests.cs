#if UNITY_EDITOR
using NUnit.Framework;

namespace EasyIdleGame.Tests
{
    public static class BigNumberTests
    {
        [Test]
        public static void Initialization()
        {
            Assert.IsTrue(new BigNumber(1, 0).ToStringDebug() == "1e0", "Initialization of 1e0 failed");
            Assert.IsTrue(new BigNumber(1.1, 2).ToStringDebug() == "1.1e2", "Initialization of 1.1e2 failed");

            // precision tests
            Assert.IsTrue(new BigNumber(1.234567, 8).ToStringDebug() == "1.234567e8", "Precision initialization of 1.234567e8 failed");
            Assert.IsTrue(new BigNumber(123_456_789).ToStringDebug() == "1.23456789e8", "Precision initialization from integer 123_456_789 failed");
        }

        [Test]
        public static void Comparison()
        {
            Assert.IsTrue(new BigNumber(1, 0) > 0, "1e0 should be greater than 0");
            Assert.IsFalse(new BigNumber(-1, 0) > 0, "-1e0 should not be greater than 0");

            Assert.IsTrue(new BigNumber(1, 0) >= 0, "1e0 should be greater or equal to 0");
            Assert.IsFalse(new BigNumber(-1, 0) >= 0, "-1e0 should not be greater or equal to 0");

            Assert.IsFalse(new BigNumber(1, 0) == 0, "1e0 should not equal 0");
            Assert.IsFalse(new BigNumber(-1, 0) == 0, "-1e0 should not equal 0");

            Assert.IsFalse(new BigNumber(1, 0) < 0, "1e0 should not be less than 0");
            Assert.IsTrue(new BigNumber(-1, 0) < 0, "-1e0 should be less than 0");

            Assert.IsFalse(0 > new BigNumber(1, 1), "0 should not be greater than 1e1");
            Assert.IsFalse(0 >= new BigNumber(1, 1), "0 should not be greater or equal to 1e1");

            Assert.IsFalse(0 == new BigNumber(1, 1), "0 should not equal 1e1");
            Assert.IsTrue(0 < new BigNumber(1, 1), "0 should be less than 1e1");

            Assert.IsFalse(new BigNumber(1, 0) > new BigNumber(1, 0), "1e0 should not be greater than 1e0");
            Assert.IsTrue(new BigNumber(1, 0) >= new BigNumber(1, 0), "1e0 should be greater or equal to 1e0");

            Assert.IsTrue(new BigNumber(1, 0) == new BigNumber(1, 0), "1e0 should equal 1e0");
            Assert.IsFalse(new BigNumber(1, 0) < new BigNumber(1, 0), "1e0 should not be less than 1e0");

            Assert.IsTrue(new BigNumber(6.58, 8) > new BigNumber(-1, 0), "6.58e8 should be greater than -1e0");
            Assert.IsTrue(new BigNumber(6.58, 8) > new BigNumber(1, 0), "6.58e8 should be greater than 1e0");

            Assert.IsTrue(new BigNumber(-1, 0) > new BigNumber(-1, 3), "-1e0 should be greater than -1e3");

            Assert.IsTrue(new BigNumber(3.03, 3) == 3030, "3.03e3 should equal 3030");
            Assert.IsTrue(new BigNumber(3.03, 9) == 3_030_000_000, "3.03e9 should equal 3_030_000_000");
        }

        [Test]
        public static void ConstanstsComparison()
        {
            Assert.IsTrue(BigNumber.MaxValue > new BigNumber(1, 0), "MaxValue should be greater than 1e0");
            Assert.IsTrue(BigNumber.MinValue < new BigNumber(-1, 0), "MinValue should be less than -1e0");
            Assert.IsTrue(BigNumber.MaxValue > BigNumber.MinValue, "MaxValue should be greater than MinValue");

            Assert.IsTrue(1_000 < BigNumber.MaxValue, "1000 should be less than MaxValue");
        }

        [Test]
        public static void PrecisionComparison()
        {
            Assert.IsTrue(new BigNumber(1.5534, 0) > new BigNumber(1.5533, 0), "1.5534e0 should be greater than 1.5533e0");
            Assert.IsTrue(new BigNumber(1.23436, 11) == new BigNumber(1.23436, 11), "1.23436e11 should equal 1.23436e11");
            Assert.IsTrue(new BigNumber(1.00034996, 3) > new BigNumber(1, 3), "1.00034996e3 should be greater than 1e3");
        }

        [Test]
        public static void Addition()
        {
            Assert.IsTrue(new BigNumber(1, 0) + new BigNumber(1, 0) == new BigNumber(2, 0), "1e0 + 1e0 should equal 2e0");
            Assert.IsTrue(new BigNumber(1, 0) + new BigNumber(1, 1) == new BigNumber(1.1, 1), "1e0 + 1e1 should equal 1.1e1");

            Assert.IsTrue(new BigNumber(1.1, 0) + new BigNumber(1.1, 0) == new BigNumber(2.2, 0), "1.1e0 + 1.1e0 should equal 2.2e0");

            // precision tests
            Assert.IsTrue(new BigNumber(1.13, 0) + new BigNumber(5.2343, 0) == new BigNumber(6.3643, 0), "1.13e0 + 5.2343e0 should equal 6.3643e0");
            Assert.IsTrue(new BigNumber(1.13, 12) + new BigNumber(5.2343, 12) == new BigNumber(6.3643, 12), "1.13e12 + 5.2343e12 should equal 6.3643e12");
        }

        [Test]
        public static void Substraction()
        {
            Assert.IsTrue(new BigNumber(1, 0) - new BigNumber(1, 0) == new BigNumber(0, 0), "1e0 - 1e0 should equal 0e0");
            Assert.IsTrue(new BigNumber(1, 0) - new BigNumber(1, 1) == new BigNumber(-.9, 1), "1e0 - 1e1 should equal -.9e1");
            Assert.IsTrue(new BigNumber(1.1, 0) - new BigNumber(1.1, 0) == new BigNumber(0, 0), "1.1e0 - 1.1e0 should equal 0e0");

            // precision tests
            Assert.IsTrue(new BigNumber(1.13, 0) - new BigNumber(5.2343, 0) == new BigNumber(-4.1043, 0), "1.13e0 - 5.2343e0 should equal -4.1043e0");
            Assert.IsTrue(new BigNumber(1.13, 12) - new BigNumber(5.2343, 12) == new BigNumber(-4.1043, 12), "1.13e12 - 5.2343e12 should equal -4.1043e12");
        }

        [Test]
        public static void Conversion()
        {
            Assert.IsTrue((BigNumber)1 == new BigNumber(1, 0), "Conversion from int 1 should equal 1e0");
            Assert.IsTrue((BigNumber)1.1 == new BigNumber(1.1, 0), "Conversion from double 1.1 should equal 1.1e0");
            Assert.IsTrue((BigNumber)1.1f == new BigNumber(1.1, 0), "Conversion from float 1.1f should equal 1.1e0");
            Assert.IsTrue((BigNumber)1.1d == new BigNumber(1.1, 0), "Conversion from double 1.1d should equal 1.1e0");
            Assert.IsTrue((BigNumber)1L == new BigNumber(1, 0), "Conversion from long 1L should equal 1e0");
            Assert.IsTrue((BigNumber)1m == new BigNumber(1, 0), "Conversion from decimal 1m should equal 1e0");
        }

        [Test]
        public static void Multiplication()
        {
            Assert.IsTrue(new BigNumber(1, 0) * new BigNumber(1, 0) == new BigNumber(1, 0), "1e0 * 1e0 should equal 1e0");
            Assert.IsTrue(new BigNumber(1, 0) * new BigNumber(1, 1) == new BigNumber(1, 1), "1e0 * 1e1 should equal 1e1");

            Assert.IsTrue(new BigNumber(1.1, 0) * new BigNumber(1.1, 0) == new BigNumber(1.21, 0), "1.1e0 * 1.1e0 should equal 1.21e0");
            Assert.IsTrue(new BigNumber(1.1, 0) * new BigNumber(1.1, 1) == new BigNumber(1.21, 1), "1.1e0 * 1.1e1 should equal 1.21e1");

            Assert.IsTrue((new BigNumber(7.58, 59) * new BigNumber(100, 0) / 100) == new BigNumber(7.58, 59), "Multiplication and division should return original value");

            // precision tests
            Assert.IsTrue(new BigNumber(1.13, 0) * new BigNumber(5.2343, 0) == new BigNumber(5.914759, 0), "1.13e0 * 5.2343e0 should equal 5.914759e0");

            // small numbers
            Assert.IsTrue(new BigNumber(0.001d) * new BigNumber(0.001d) == new BigNumber(0.000001d), "0.001 * 0.001 should equal 0.000001");
            Assert.IsTrue(new BigNumber(1, -3) * new BigNumber(1, -3) == new BigNumber(1, -6), "1e-3 * 1e-3 should equal 1e-6");
            Assert.IsTrue(new BigNumber(0.005d) * new BigNumber(1000) == new BigNumber(5, 0), "0.005 * 1000 should equal 5");
            Assert.IsTrue(new BigNumber(0.00000123d) * new BigNumber(2) == new BigNumber(0.00000246d), "0.00000123 * 2 should equal 0.00000246");
        }

        [Test]
        public static void Flooring()
        {
            // floor to 1
            Assert.IsTrue(new BigNumber(1.1, 0).Floor() == new BigNumber(1, 0), "1.1e0 floored should equal 1e0");
            Assert.IsTrue(new BigNumber(1.155, 0).Floor() == new BigNumber(1, 0), "1.155e0 floored should equal 1e0");
            Assert.IsTrue(new BigNumber(1.655, 0).Floor() == new BigNumber(1, 0), "1.655e0 floored should equal 1e0");

            // floor to 2
            Assert.IsTrue(new BigNumber(2.124, 0).Floor() == new BigNumber(2, 0), "2.124e0 floored should equal 2e0");

            // floor to 10
            Assert.IsTrue(new BigNumber(1.1, 1).Floor() == new BigNumber(1.1, 1), "1.1e1 floored should equal 1.1e1");
            Assert.IsTrue(new BigNumber(1.155, 1).Floor() == new BigNumber(1.1, 1), "1.155e1 floored should equal 1.1e1");

            Assert.IsTrue(new BigNumber(2.781, 18).Floor() == new BigNumber(2.781, 18), "2.781e18 floored should equal 2.781e18");

            Assert.IsTrue(new BigNumber(7.58, 59).Floor() == new BigNumber(7.58, 59), "7.58e59 floored should equal 7.58e59");

            Assert.IsTrue((new BigNumber(7.58, 59) * new BigNumber(100, 0) / 100).Floor() == new BigNumber(7.58, 59), "Operation and floored should equal 7.58e59");

            Assert.IsTrue(new BigNumber(1.155, 136).Floor() == new BigNumber(1.155, 136), "1.155e136 floored should equal 1.155e136");

            Assert.IsTrue(new BigNumber(9.5, 0).Floor() == new BigNumber(9, 0), "9.5e0 floored should equal 9e0");

            Assert.IsTrue(new BigNumber(1.76552, 62).Floor() == new BigNumber(1.76552, 62), "1.76552e62 floored should equal 1.76552e62");
        }

        [Test]
        public static void Rounding()
        {
            Assert.IsTrue(new BigNumber(1.1, 0).Round(0) == new BigNumber(1, 0), "1.1e0 rounded to 0 should equal 1e0");
            Assert.IsTrue(new BigNumber(0.45, 0).Round(2) == new BigNumber(0.45, 0), "0.45e0 rounded to 2 should equal 0.45e0");
            Assert.IsTrue(new BigNumber(0.46, 0).Round(1) == new BigNumber(0.5, 0), "0.46e0 rounded to 1 should equal 0.5e0");
            Assert.IsTrue(new BigNumber(4.78297, -1).Round(2) == new BigNumber(0.48, 0), "4.78297e-1 rounded to 2 should equal 0.48e0");
        }

        [Test]
        public static void RoundToWholeNumber()
        {
            Assert.IsTrue(new BigNumber(1.1, 0).RoundToWholeNumber() == new BigNumber(1, 0), "1.1e0 rounded to whole should equal 1e0");
            Assert.IsTrue(new BigNumber(1.1, 3).RoundToWholeNumber() == new BigNumber(1.1, 3), "1.1e3 rounded to whole should equal 1.1e3");
        }

        [Test]
        public static void Ceiling()
        {
            Assert.IsTrue(new BigNumber(1.1, 0).Ceil() == new BigNumber(2, 0), "1.1e0 ceiling should equal 2e0");
            Assert.IsTrue(new BigNumber(1.155, 0).Ceil() == new BigNumber(2, 0), "1.155e0 ceiling should equal 2e0");
            Assert.IsTrue(new BigNumber(1.655, 1).Ceil() == new BigNumber(1.7, 1), "1.655e1 ceiling should equal 1.7e1");
        }

        [Test]
        public static void Pow()
        {
            Assert.IsTrue(new BigNumber(1, 0).Pow(2) == new BigNumber(1, 0), "1e0^2 should equal 1e0");
            Assert.IsTrue(new BigNumber(1, 1).Pow(0) == new BigNumber(1, 0), "1e1^0 should equal 1e0");

            Assert.IsTrue(new BigNumber(1, 1).Pow(2) == new BigNumber(1, 2), "1e1^2 should equal 1e2");
            Assert.IsTrue(new BigNumber(1, 1).Pow(10) == new BigNumber(1, 10), "1e1^10 should equal 1e10");

            Assert.IsTrue(new BigNumber(1.1, 1).Pow(2) == new BigNumber(1.21, 2), "1.1e1^2 should equal 1.21e2");

            Assert.IsTrue(new BigNumber(2, 15).Pow(10) == new BigNumber(1024, 150), "2e15^10 should equal 1024e150");

            Assert.IsTrue(new BigNumber(1, 0).Pow(-1) == new BigNumber(1, 0), "1e0^-1 should equal 1e0");
            Assert.IsTrue(new BigNumber(1, 1).Pow(-1) == new BigNumber(.1, 0), "1e1^-1 should equal .1e0");

            Assert.IsTrue(new BigNumber(1, 1).Pow(.5) == new BigNumber(1, 0), "1e1^0.5 should equal 1e0");

            Assert.IsTrue(new BigNumber(2, 0).Pow(1_000).Round(3) == new BigNumber(1.072, 301), "2e0^1000 should equal 1.072e301");
            Assert.IsTrue(new BigNumber(2, 0).Pow(10_000).Round(3) == new BigNumber(1.995, 3010), "2e0^10000 should equal 1.995e3010");
            Assert.IsTrue(new BigNumber(2, 0).Pow(100_000).Round(3) == new BigNumber(9.990, 30102), "2e0^100000 should equal 9.990e30102");
            Assert.IsTrue(new BigNumber(2, 0).Pow(1_000_000).Round(3) == new BigNumber(9.901, 301029), "2e0^1000000 should equal 9.901e301029");
            Assert.IsTrue(new BigNumber(2, 0).Pow(1_000_000_000_000).Round(3) == new BigNumber(5.332, 301029995674), "2e0^1000000000000 should equal 5.332e301029995674");
            Assert.IsTrue(new BigNumber(2, 0).Pow(1_000_000_000_000_000).Round(3) == new BigNumber(8.413, 301029995674726), "2e0^1000000000000000 should equal 8.413e301029995674726");

            Assert.IsTrue(new BigNumber(5, 0).Pow(16).Round(3) == new BigNumber(1.526, 11), "5e0^16 should equal 1.526e11");
        }

        [Test]
        public static void Modulo()
        {
            Assert.IsTrue(new BigNumber(1, 0) % new BigNumber(1, 0) == 0, "1e0 % 1e0 should equal 0");
            Assert.IsTrue(new BigNumber(1, 0) % new BigNumber(1, 1) == 1, "1e0 % 1e1 should equal 1");
            Assert.IsTrue(new BigNumber(10, 0) % new BigNumber(3, 0) == 1, "10e0 % 3e0 should equal 1");
            Assert.IsTrue(new BigNumber(5, 3) % new BigNumber(2, 2) == 0, "5e3 % 2e2 should equal 0");

            Assert.IsTrue(new BigNumber(7, 3) % new BigNumber(3, 2) == 100, "7e3 % 3e2 should equal 100");

            Assert.IsTrue(new BigNumber(1, 100) % new BigNumber(10, 0) == 0, "1e100 % 10e0 should equal 0");
            Assert.IsTrue(new BigNumber(9, 2) % new BigNumber(2, 1) == 0, "9e2 % 2e1 should equal 0");
            Assert.IsTrue(new BigNumber(123, 5) % new BigNumber(50, 5) == 2300000, "123e5 % 50e5 should equal 2300000");
        }
    }
}
#endif