using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Managers
{
    /// <summary>
    /// Bridges the same confirmed native gap as RareLegendaryManagerProductionModifier.cs
    /// (see that file's header for the full root-cause explanation: a Manager's own
    /// passiveBoosts never reach anything unless that Manager IS a Business's own
    /// dedicated `.manager`) for effect types that are NOT part of a production event, so
    /// they can't be fixed through Easy Idle Game's production-modifier extension point.
    ///
    /// Currently handles one responsibility:
    ///
    /// 1. The Growth Hacker (+X%/lvl global player XP): PlayerStats.AddXp only ever
    ///    multiplies incoming XP by UpgradesManager's and BoostsManager's multipliers -
    ///    confirmed via full read of PlayerStats.cs, it never queries ManagersManager at
    ///    all, so no per-business or per-output hook could reach it even in principle.
    ///    Instead, this listens to PlayerStats.OnXpAdded (fired with the XP amount AFTER
    ///    the Upgrades/Boosts multipliers already applied, but before that amount is
    ///    credited to the player's level) and tops up with the extra XP The Growth Hacker's
    ///    level-scaled multiplier is owed, via a second AddXp call - the same manager
    ///    method the rest of the game already uses to grant XP (see CLAUDE.md Hard Rule
    ///    #4). A reentrancy guard prevents that top-up call's own OnXpAdded firing from
    ///    granting a second, third, ... top-up on top of itself.
    ///
    /// KNOWN IMPRECISION: the top-up amount itself passes back through AddXp's own
    /// Upgrades/Boosts multiplier a second time (there's no way to add XP that skips that
    /// step without touching PlayerStats.cs directly). With no Upgrades or Boosts
    /// currently targeting UpgradeType.playerXp other than through Growth Hacker itself,
    /// this has no observable effect today, but if a playerXp-boosting Upgrade or Boost is
    /// ever added, Growth Hacker's effective bonus will compound very slightly high versus
    /// a literal reading of "+X%/lvl". Flagged here, not fixed - low priority, and fixing
    /// it properly needs the same vendored PlayerStats.AddXp access already used once
    /// under an approved scoped exception (see CLAUDE.md Watch-list).
    ///
    /// WHY OTHER MANAGERS AREN'T HANDLED HERE: The Budget Bestie (purchaseCost) and The
    /// Algorithm Itself (speed) have no equivalent discrete "grant" event to intercept -
    /// their multipliers are read live and repeatedly from Business.GetBusinessMultiplierOfType
    /// wherever cost or remaining production time is displayed or checked, with no single
    /// hook point reachable from outside that vendored method. The Nepo Baby (dropWeight,
    /// Capsule pull odds) resolves synchronously inside ShopItemHolder.ClaimRewards with no
    /// pre-roll event to intercept either, and the only way found to influence it without
    /// touching vendored code would be temporarily mutating the shared Capsule ShopItem
    /// asset's Output.weight fields at claim time - rejected as too fragile a pattern for
    /// shared asset data. All three are documented as open findings rather than built
    /// around with something equally fragile - see docs/BugTracker.md.
    /// </summary>
    [AddComponentMenu("InfluencerRise/Rare-Legendary Manager Effects")]
    public class RareLegendaryManagerEffects : MonoBehaviour
    {
        [Tooltip("The Growth Hacker manager. While owned, tops up XP grants with the extra amount its level-scaled playerXp multiplier is owed. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager growthHacker;

        private bool _isApplyingGrowthHackerBonus;

        private void OnEnable()
        {
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.OnXpAdded.AddListener(HandleXpAdded);
        }

        private void OnDisable()
        {
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.OnXpAdded.RemoveListener(HandleXpAdded);
        }

        /// <summary>Responsibility 1: tops up XP with whatever extra The Growth Hacker's level-scaled multiplier is owed.</summary>
        private void HandleXpAdded(BigNumber xps)
        {
            if (_isApplyingGrowthHackerBonus) return;
            if (growthHacker == null || ManagersManager.Instance == null || PlayerStats.Instance == null) return;

            ManagerHolder holder = ManagersManager.Instance.GetHolder(growthHacker);
            if (holder == null || holder.level <= 0) return;

            float multiplier = holder.GetMultiplierOfType(UpgradeType.playerXp);
            if (multiplier <= 1f) return;

            BigNumber bonusXp = xps * (BigNumber)(multiplier - 1f);
            if (bonusXp <= 0) return;

            _isApplyingGrowthHackerBonus = true;
            PlayerStats.Instance.AddXp(bonusXp);
            _isApplyingGrowthHackerBonus = false;
        }
    }
}
