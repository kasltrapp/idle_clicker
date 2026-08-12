using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newShopItemGroup", menuName = "EasyIdleGame/Shop/Shop Item Group")]
    public class ShopItemGroup : ItemGroup<ShopItem>
    {
        [CommentArea("ShopItem Group", "Groups multiple shop items under a single group.", "Assign this group to each ShopItem that belongs to the group.")]
        [SerializeField] private string _shopItemGroupComment;
    }
}
