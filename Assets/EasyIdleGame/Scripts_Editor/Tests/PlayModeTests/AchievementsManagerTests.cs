using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace EasyIdleGame.Tests
{
    public class AchievementsManagerTests
    {
        private GameObject _managerObj;

        [SetUp]
        public void SetUp()
        {
            _managerObj = TestUtilities.SetupManagerObject();
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.TearDownManagerObject(_managerObj);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator AutoClaim_FiresEventAndGrantsRewardsWhenUnlocked()
        {
            var achievement = ScriptableObject.CreateInstance<Achievement>();
            achievement.autoClaim = true;
            achievement.locks = new List<Lock>(); // Always unlocked

            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;
            achievement.rewardTables = new List<DropTable> { new DropTable {
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output() { currencyOutput = rewardCurrency, outputAmount = 100 } }
            }};

            var holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
            
            bool eventFired = false;
            AchievementsManager.Instance.OnAchievementClaimed.AddListener((a) => { if (a == achievement) eventFired = true; });

            Assert.AreEqual(new BigNumber(0), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Reward currency should initially be 0");
            Assert.IsFalse(holder.isClaimed, "Achievement should not be claimed yet");

            // Simulate the update loop passing 1 second
            var updateMethod = typeof(AchievementsManager).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timeField = typeof(AchievementsManager).GetField("_timeSinceLastCheck", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            timeField.SetValue(AchievementsManager.Instance, 10f); // Fast forward time
            updateMethod.Invoke(AchievementsManager.Instance, null);

            Assert.IsTrue(holder.isClaimed, "Achievement should be claimed after update loop");
            yield return new WaitForSeconds(1.1f); // Wait for Update loop (timeSinceLastCheck >= 1f)

            Assert.IsTrue(eventFired, "OnAchievementClaimed event should fire");
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Should grant 100 reward currency automatically");

            // Verify it doesn't double claim
            eventFired = false;
            timeField.SetValue(AchievementsManager.Instance, 10f);
            updateMethod.Invoke(AchievementsManager.Instance, null);

            Assert.IsFalse(eventFired, "Event should not fire again");
            Assert.AreEqual(new BigNumber(100), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Currency should remain 100");

            Object.DestroyImmediate(achievement);
            Object.DestroyImmediate(rewardCurrency);
        }

        [Test]
        public void ClaimAchievement_ValidatesAlreadyClaimed()
        {
            var achievement = ScriptableObject.CreateInstance<Achievement>();
            achievement.locks = new List<Lock>(); // Unlocked
            
            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;
            achievement.rewardTables = new List<DropTable> { new DropTable {
                strategy = DropStrategy.All,
                outputs = new List<Output> { new Output() { currencyOutput = rewardCurrency, outputAmount = 50 } }
            }};

            var holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
            
            Assert.AreEqual(new BigNumber(50), achievement.rewardTables[0].outputs[0].GetAmount(), "Output amount should be 50 before claim");

            AchievementsManager.Instance.TryClaimAchievement(achievement, out _);
            Assert.IsTrue(holder.isClaimed, "Achievement should be claimed");
            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Should grant 50 reward currency");

            // Try claiming again
            AchievementsManager.Instance.TryClaimAchievement(achievement, out _);
            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetOrAddHolder(rewardCurrency).amount, "Should not grant reward twice when already claimed");

            Object.DestroyImmediate(achievement);
            Object.DestroyImmediate(rewardCurrency);
        }

        [Test]
        public void ClaimAchievement_UsesLifetimeCurrencyAmountForTotalAmountLock()
        {
            var achievement = ScriptableObject.CreateInstance<Achievement>();

            var progressCurrency = ScriptableObject.CreateInstance<Currency>();
            progressCurrency.maxAmount = -1;
            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;

            achievement.locks = new List<Lock>
            {
                new Lock
                {
                    currencyLock = progressCurrency,
                    inputAmount = 40,
                    amountType = LockAmountType.TotalAmount
                }
            };
            achievement.rewardTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output> { new Output { currencyOutput = rewardCurrency, outputAmount = 1 } }
                }
            };

            CurrencyManager.Instance.AddCurrencyAmount(progressCurrency, 50);
            CurrencyManager.Instance.RemoveCurrencyAmount(progressCurrency, 30);

            Assert.AreEqual(new BigNumber(20), CurrencyManager.Instance.GetCurrencyAmount(progressCurrency), "Current balance should be below the achievement threshold after spending.");
            Assert.AreEqual(new BigNumber(50), CurrencyManager.Instance.GetTotalCurrencyAmount(progressCurrency), "Lifetime currency total should retain the full earned amount.");
            Assert.IsTrue(achievement.IsCurrentlyUnlocked(), "Achievement should unlock from lifetime currency total, not current balance.");
            Assert.IsTrue(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Achievement should claim when lifetime currency total meets the lock.");
            Assert.AreEqual(new BigNumber(1), CurrencyManager.Instance.GetCurrencyAmount(rewardCurrency), "Reward should be granted after lifetime currency lock claim.");

            Object.DestroyImmediate(achievement);
            Object.DestroyImmediate(progressCurrency);
            Object.DestroyImmediate(rewardCurrency);
        }

        [Test]
        public void ClaimAchievement_AllowsMultipleScaledTiers()
        {
            var achievement = ScriptableObject.CreateInstance<Achievement>();
            achievement.claimAmount = 3;
            achievement.claimMultiplier = 10f;

            var progressCurrency = ScriptableObject.CreateInstance<Currency>();
            progressCurrency.maxAmount = -1;
            var rewardCurrency = ScriptableObject.CreateInstance<Currency>();
            rewardCurrency.maxAmount = -1;

            achievement.locks = new List<Lock>
            {
                new Lock
                {
                    currencyLock = progressCurrency,
                    inputAmount = 100,
                    amountType = LockAmountType.TotalAmount
                }
            };
            achievement.rewardTables = new List<DropTable>
            {
                new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output> { new Output { currencyOutput = rewardCurrency, outputAmount = 5 } }
                }
            };

            AchievementHolder holder = AchievementsManager.Instance.GetOrAddHolder(achievement);

            Assert.AreEqual(new BigNumber(100), holder.GetCurrentTierLocks()[0].inputAmount, "First tier should use the configured lock amount.");
            Assert.IsFalse(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Tier should not claim before the first threshold is met.");

            CurrencyManager.Instance.AddCurrencyAmount(progressCurrency, 100);
            Assert.IsTrue(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "First tier should claim at 100 total currency.");
            Assert.AreEqual(1, holder.ClaimedAmount);
            Assert.IsFalse(holder.isClaimed, "Achievement should remain available for later tiers.");
            Assert.AreEqual(new BigNumber(1000), holder.GetCurrentTierLocks()[0].inputAmount, "Second tier should scale by claimMultiplier.");
            Assert.AreEqual(new BigNumber(5), CurrencyManager.Instance.GetCurrencyAmount(rewardCurrency));
            Assert.IsFalse(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Second tier should wait for the scaled threshold.");

            CurrencyManager.Instance.AddCurrencyAmount(progressCurrency, 900);
            Assert.IsTrue(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Second tier should claim at 1000 total currency.");
            Assert.AreEqual(2, holder.ClaimedAmount);
            Assert.AreEqual(new BigNumber(10000), holder.GetCurrentTierLocks()[0].inputAmount, "Third tier should scale geometrically.");

            CurrencyManager.Instance.AddCurrencyAmount(progressCurrency, 9000);
            Assert.IsTrue(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Third tier should claim at 10000 total currency.");
            Assert.AreEqual(3, holder.ClaimedAmount);
            Assert.IsTrue(holder.isClaimed, "Achievement should be fully claimed after all configured tiers.");
            Assert.AreEqual(new BigNumber(15), CurrencyManager.Instance.GetCurrencyAmount(rewardCurrency), "Rewards should be granted once per claimed tier.");
            Assert.IsFalse(AchievementsManager.Instance.TryClaimAchievement(achievement, out _), "Achievement should stop claiming after claimAmount tiers.");

            Object.DestroyImmediate(achievement);
            Object.DestroyImmediate(progressCurrency);
            Object.DestroyImmediate(rewardCurrency);
        }
    }
}
