using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newBusinessGroup", menuName = "EasyIdleGame/Businesses/Business Group")]
    public class BusinessGroup : ItemGroup<Business>
    {
        [CommentArea("Business Group", "Groups multiple businesses under a single combined limit and gives upgrades, boosts, and managers a typed target. -1 capacity means infinite.", "Create a Farm Businesses group, assign it to Wheat Field and Cow Barn businesses, then target that group from a Manager, Upgrade, or Boost to affect both businesses.")]
        [SerializeField] private string _businessGroupComment;

        [Tooltip("Shared owned amount limit across all businesses in this group. -1 means infinite; every assigned business contributes to the same used capacity.")]
        public BigNumber capacity = -1;
    }
}
