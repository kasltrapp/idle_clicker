using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newAchievementGroup", menuName = "EasyIdleGame/Player Stats/Achievement Group")]
    public class AchievementGroup : ItemGroup<Achievement>
    {
        [CommentArea("Achievement Group", "Groups multiple achievements under a single group.", "Assign this group to each Achievement that belongs to the group.")]
        [SerializeField] private string _achievementGroupComment;
    }
}
