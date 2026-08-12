using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "AutoProduceBoost", menuName = "EasyIdleGame/Boosts/Auto Produce Boost")]
    public class AutoProduceBoost : Boost, IMergableBoost
    {
        [Header("Boost settings")]
        [CommentArea("Auto-Produce Logic", "This boost automatically triggers production cycles for target business groups while active, bypassing the need for a manager or manual clicks.", "Use it for a temporary automation reward: set duration to 15 minutes and assign the Farm business group so every farm business produces automatically during that time.")]
        [SerializeField] private string _autoProduceBoostComment;
        [Tooltip("How long target businesses auto-produce after this boost is used. Matching active boosts merge by adding their remaining duration.")]
        public Duration duration = new Duration(0, 0, 5, 0);

        [Header("Deprecated")]
        [Tooltip("[DEPRECATED] Legacy string group used by old assets and APIs. Use targetBusinessGroups for new content.")]
        [System.Obsolete("Use targetBusinessGroups instead.")]
        public string group;

        [Tooltip("Business groups that should auto-produce while this boost is active. Leave empty to affect all businesses; multiple groups target any matching business.")]
        public List<BusinessGroup> targetBusinessGroups = new List<BusinessGroup>();

        public override ActiveBoostHolder Activate()
        {
            return new ActiveBoostHolder(this, duration.TotalSeconds);
        }

        public bool CanMergeBoost(ActiveBoostHolder other)
        {
            if (other.boost is not AutoProduceBoost mBoost || mBoost.group != group) return false;

            if (mBoost.targetBusinessGroups == null && targetBusinessGroups != null) return false;
            if (mBoost.targetBusinessGroups != null && targetBusinessGroups == null) return false;
            if (mBoost.targetBusinessGroups != null && targetBusinessGroups != null && !mBoost.targetBusinessGroups.SequenceEqual(targetBusinessGroups)) return false;

            return true;
        }

        public void MergeBoost(ActiveBoostHolder self, ActiveBoostHolder other)
        {
            if (!CanMergeBoost(other)) throw new System.Exception("Can't merge boosts");
            if (other.boost is not AutoProduceBoost) throw new System.Exception("Boosts do not have the same type");

            self.timeLeft = self.timeLeft + other.timeLeft;
        }
    }
}
