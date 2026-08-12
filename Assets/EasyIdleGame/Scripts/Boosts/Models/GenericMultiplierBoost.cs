using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "GenericMultiplierBoost", menuName = "EasyIdleGame/Boosts/Generic Multiplier Boost")]
    public class GenericMultiplierBoost : Boost, IMergableBoost, IMultiplierProvider
    {
        public IMultiplierProvider iMultiplierProvider => this;

        // IMultiplierProvider
        public UpgradeBlock[] Blocks => upgradeBlocks;

        [System.Obsolete("Use TargetBusinessGroups instead.")]
        public string Group => group;
        public List<BusinessGroup> TargetBusinessGroups => targetBusinessGroups;

        [Header("Boost settings")]
        [CommentArea("Multiplier Boost", "Applies timed UpgradeBlock multipliers while active. Matching boosts merge by extending their remaining duration.", "Create a 2x Production Boost by adding a production UpgradeBlock with value 2 and setting duration to 10 minutes. Leave targetBusinessGroups empty to affect all businesses, or assign groups to affect only those businesses.")]
        [SerializeField] private string _multiplierBoostComment;
        [ConditionalCommentArea("Boost setup", "No upgrade blocks are configured. This boost can still activate and track duration, but it will not apply multiplier effects.", "upgradeBlocks", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _upgradeBlocksInfo;
        [Tooltip("Multiplier effects applied while this boost is active. Empty array means the boost can activate and merge time but changes no stats.")]
        public UpgradeBlock[] upgradeBlocks;

        [Tooltip("How long this boost remains active after it is used. Matching active boosts merge by adding their remaining duration when mergeRepeatedUses is enabled.")]
        public Duration duration = new Duration(0, 0, 5, 0);

        [Tooltip("When enabled, using a matching boost again extends the existing active timer. When disabled, each use creates a separate active boost entry.")]
        public bool mergeRepeatedUses = true;

        [Header("Deprecated")]
        [Tooltip("[DEPRECATED] Legacy string group used by old assets and APIs. Use targetBusinessGroups for new content.")]
        [System.Obsolete("Use targetBusinessGroups instead.")]
        public string group;

        [Tooltip("Business groups affected by this boost. Leave empty to affect all businesses; multiple groups target any matching business.")]
        public List<BusinessGroup> targetBusinessGroups = new List<BusinessGroup>();

        public override ActiveBoostHolder Activate()
        {
            return new ActiveBoostHolder(this, duration.TotalSeconds);
        }

        public bool CanMergeBoost(ActiveBoostHolder other)
        {
            if (!mergeRepeatedUses) return false;
            if (other.boost is not GenericMultiplierBoost mBoost || mBoost.group != group) return false;

            if (mBoost.targetBusinessGroups == null && targetBusinessGroups != null) return false;
            if (mBoost.targetBusinessGroups != null && targetBusinessGroups == null) return false;
            if (mBoost.targetBusinessGroups != null && targetBusinessGroups != null && !mBoost.targetBusinessGroups.SequenceEqual(targetBusinessGroups)) return false;

            foreach (var block in upgradeBlocks)
            {
                if (!mBoost.upgradeBlocks.Contains(block)) return false;
            }

            return true;
        }

        public void MergeBoost(ActiveBoostHolder self, ActiveBoostHolder other)
        {
            if (!CanMergeBoost(other)) throw new System.Exception("Can't merge boosts");
            if (other.boost is not GenericMultiplierBoost) throw new System.Exception("Boosts do not have the same type");

            self.timeLeft = self.timeLeft + other.timeLeft;
        }
    }
}
