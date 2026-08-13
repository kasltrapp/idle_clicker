using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Managers
{
    /// <summary>
    /// Companion asset pairing an existing Manager with a Rarity tier.
    ///
    /// WHY A COMPANION ASSET, AND WHY NOT A FIELD DIRECTLY ON MANAGER:
    /// Manager.cs (Assets/EasyIdleGame/Scripts/Managers/Manager.cs) is vendor package code and
    /// isn't meant to be edited - a future package update would overwrite any change there.
    ///
    /// Subclassing Manager (e.g. "ManagerWithRarity : Manager") was investigated and found
    /// technically SAFE in isolation: Resources.LoadAll&lt;Manager&gt;() (the method
    /// ManagerHolderBase.GetAllScriptableObjects_Static uses to find every Manager) includes
    /// subclass instances by standard Unity polymorphism, and no custom Inspector exists for
    /// Manager anywhere in the package to break. BUT all 5 existing Manager assets
    /// (GhostwriterManager, SocialMediaManager, EditorManager, TalentAgentManager,
    /// PublicistManager) already exist as plain Manager-typed assets. Giving them a rarity via
    /// subclassing would mean re-typing already-created, name-locked assets in place - a far
    /// more invasive change than adding new, purely-additive companion data alongside them.
    ///
    /// Manager.managerGroups (List&lt;ManagerGroup&gt;) was also considered as an attachment
    /// point (e.g. a "ManagerRarity : ManagerGroup" subclass added to that existing list), but
    /// that field is an ACTIVE functional dependency of PrestigeManager.excludedManagerGroups
    /// (see PrestigeManager.cs ResetProgress -> AddItemsInGroups) - mixing rarity tags into it
    /// risks a rarity tier accidentally being used for (or excluded from) Rebrand reset scoping
    /// later. Kept fully separate to avoid that collision.
    ///
    /// The reference direction is therefore inverted from a simple "Manager points at its
    /// rarity": this asset points AT the Manager instead, since Manager itself can't be given a
    /// new field without touching vendor code.
    /// </summary>
    [CreateAssetMenu(fileName = "newManagerRarity", menuName = "InfluencerRise/Manager Rarity")]
    public class ManagerRarity : ScriptableObject
    {
        [Tooltip("The Manager asset this rarity assignment applies to.")]
        public Manager manager;

        [Tooltip("The rarity tier assigned to this Manager.")]
        public Rarity rarity;
    }
}
