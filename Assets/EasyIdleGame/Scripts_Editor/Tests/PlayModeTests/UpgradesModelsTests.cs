using NUnit.Framework;

namespace EasyIdleGame.Tests
{
    public class UpgradesModelsTests
    {
        [Test]
        public void UpgradeBlock_CompareTo_ComparesCorrectly()
        {
            UpgradeBlock block1 = new UpgradeBlock(UpgradeType.production, 2f);
            UpgradeBlock block2 = new UpgradeBlock(UpgradeType.production, 2f);
            UpgradeBlock block3 = new UpgradeBlock(UpgradeType.speed, 2f);
            UpgradeBlock block4 = new UpgradeBlock(UpgradeType.production, 3f);

            Assert.AreEqual(0, block1.CompareTo(block2), "Identical blocks should return 0.");
            Assert.AreEqual(-1, block1.CompareTo(block3), "Different types should return -1.");
            Assert.AreEqual(-1, block1.CompareTo(block4), "Different values should return -1.");
        }

        [Test]
        public void OverrideUpgradeBlock_CompareTo_ComparesCorrectly()
        {
            OverrideUpgradeBlock block1 = new OverrideUpgradeBlock(OverrideUpgradeType.maxCurrencyAmount, 100);
            OverrideUpgradeBlock block2 = new OverrideUpgradeBlock(OverrideUpgradeType.maxCurrencyAmount, 100);
            OverrideUpgradeBlock block3 = new OverrideUpgradeBlock(OverrideUpgradeType.maxCurrencyAmount, 200);

            Assert.AreEqual(0, block1.CompareTo(block2), "Identical blocks should return 0.");
            Assert.AreEqual(-1, block1.CompareTo(block3), "Different values should return -1.");
        }
    }
}
