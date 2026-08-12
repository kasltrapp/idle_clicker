using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newBoostGroup", menuName = "EasyIdleGame/Boosts/Boost Group")]
    public class BoostGroup : ItemGroup<Boost>
    {
        [CommentArea("Boost Group", "Groups multiple boosts under a single group.", "Assign this group to each Boost that belongs to the group.")]
        [SerializeField] private string _boostGroupComment;
    }
}
