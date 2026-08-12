using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "EasyIdleGame/Player Stats/Achievement")]
    public class Achievement : ScriptableObject, IUnlockable, ISerializationCallbackReceiver, IMetadataProvider
    {
        [CommentArea("Achievement", "Represents player milestones defined by a set of unlock conditions. Achievements are used to track progression and can trigger rewards when all locks are fulfilled.", "Create an achievement for earning 1,000 Coins by adding a currency lock with amount 1000, then add a reward table and assign the asset to an AchievementDisplayer.")]
        [SerializeField] private string _achievementComment;

        [Header("General")]
        [Tooltip("Display metadata shown by achievement UI. Fill this when achievement screens need a name, icon, or description.")]
        public Metadata metadata = new Metadata();
        public Metadata Metadata => metadata ??= new Metadata();

        [Tooltip("Typed groups this achievement belongs to. Used for shared locks and targeting; leave empty if the achievement has no specific group.")]
        public List<AchievementGroup> achievementGroups = new List<AchievementGroup>();

        [ConditionalCommentArea("Achievement setup", "No locks are configured. This achievement will be considered unlocked immediately.", "locks", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _emptyLocksInfo;
        [ConditionalCommentArea("Achievement setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this achievement unlocks. Empty list means the achievement is unlocked immediately.")]
        public List<Lock> locks = new List<Lock>();

        [ConditionalCommentArea("Achievement setup", "No rewards are configured. Claiming this achievement will not grant anything.", "rewardTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _rewardTablesInfo;
        [Tooltip("Drop tables granted when the achievement is claimed. Empty rewards are allowed but claiming grants nothing.")]
        public List<DropTable> rewardTables = new List<DropTable>();

        [Header("Claim Tiers")]
        [Tooltip("Total number of times this achievement can be claimed. Keep at 1 for a normal single-claim achievement.")]
        [Min(1)]
        public int claimAmount = 1;

        [Tooltip("Multiplier applied to lock amounts after each claim. Example: a 100 currency lock with multiplier 10 unlocks at 100, 1000, 10000.")]
        [Min(1f)]
        public float claimMultiplier = 2f;

        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("rewards")]
        public List<Output> _legacyRewards;

        [Tooltip("If enabled, the achievement claims itself as soon as it unlocks. Disable when the player should press a Claim button.")]
        public bool autoClaim = false;

        [Header("Audio")]
        [Tooltip("Sound played when this achievement is unlocked.")]
        public AudioData unlockSound;
        [Tooltip("Sound played when this achievement is claimed.")]
        public AudioData claimSound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => null;
        public bool UnlockOnlyOnce => true;
        public AudioData UnlockSound => unlockSound;

        public bool IsCurrentlyUnlocked()
        {
            if (locks == null || locks.Count == 0) return true;
            foreach (Lock lockItem in locks)
            {
                if (!lockItem.IsUnlocked())
                    return false;
            }

            return true;
        }

        public int TotalClaimAmount => Mathf.Max(1, claimAmount);

        public BigNumber GetClaimTierMultiplier(int claimedAmount)
        {
            if (claimedAmount <= 0) return 1;

            BigNumber sanitizedMultiplier = Mathf.Max(1f, claimMultiplier);
            return sanitizedMultiplier.Pow(claimedAmount);
        }

        public List<Lock> GetLocksForClaimTier(int claimedAmount)
        {
            if (locks == null || locks.Count == 0) return new List<Lock>();
            return locks.Multiply(GetClaimTierMultiplier(claimedAmount), true);
        }

        public bool IsClaimTierUnlocked(int claimedAmount)
        {
            if (claimedAmount >= TotalClaimAmount) return false;
            return GetLocksForClaimTier(claimedAmount).IsUnlocked();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (_legacyRewards != null && _legacyRewards.Count > 0)
            {
                if (rewardTables == null) rewardTables = new List<DropTable>();

                DropTable table = new DropTable();
                table.strategy = DropStrategy.All;
                table.outputs = new List<Output>(_legacyRewards);

                rewardTables.Add(table);
                _legacyRewards.Clear();
            }
        }
    }
}
