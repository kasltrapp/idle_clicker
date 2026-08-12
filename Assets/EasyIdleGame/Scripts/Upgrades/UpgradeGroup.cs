using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newUpgradeGroup", menuName = "EasyIdleGame/Upgrades/Upgrade Group")]
    public class UpgradeGroup : ItemGroup<Upgrade>
    {
        [CommentArea("Upgrade Group", "Groups multiple upgrades under a single group.", "Assign this group to each Upgrade that belongs to the group.")]
        [SerializeField] private string _upgradeGroupComment;
    }
}
