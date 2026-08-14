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
    /// Four responsibilities:
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
    /// KNOWN IMPRECISION (Responsibility 1): the top-up amount itself passes back through
    /// AddXp's own Upgrades/Boosts multiplier a second time (there's no way to add XP that
    /// skips that step without touching PlayerStats.cs directly). With no Upgrades or
    /// Boosts currently targeting UpgradeType.playerXp other than through Growth Hacker
    /// itself, this has no observable effect today, but if a playerXp-boosting Upgrade or
    /// Boost is ever added, Growth Hacker's effective bonus will compound very slightly
    /// high versus a literal reading of "+X%/lvl". Flagged here, not fixed - low priority,
    /// and fixing it properly needs the same vendored PlayerStats.AddXp access already
    /// used once under an approved scoped exception (see CLAUDE.md Watch-list).
    ///
    /// 2. The Budget Bestie - REDESIGNED (was: -15%/lvl global purchaseCost passiveBoost,
    ///    confirmed non-functional - purchaseCost is read live and repeatedly from
    ///    Business.GetBusinessMultiplierOfType wherever cost is checked or displayed, with
    ///    no discrete "grant" event to intercept the way XP-granting has one, and no
    ///    reachable hook was found without a live-read vendored patch - deliberately
    ///    avoided given the severity of the Level.AddXp fix, see CLAUDE.md Watch-list).
    ///    Native levelRewards/eachLevelReward (PlayerStats.levelupData) were investigated
    ///    as a possible zero-code alternative and confirmed NOT viable: LevelRewardHolder
    ///    (Assets/EasyIdleGame/Scripts/_Models/Levels/LevelRewards/LevelRewardHolder.cs)
    ///    has exactly two fields, `level` and `levelReward` - no ownership/condition field
    ///    of any kind, so a level-up reward cannot be gated on owning a Manager natively.
    ///    Redesigned instead as a Cash burst granted on every player level-up while owned,
    ///    scaling with BOTH the new player level and The Budget Bestie's own manager level
    ///    (consistent with every other Rare/Legendary manager's level-scaled philosophy).
    ///    Hooks PlayerStats.OnLevelUpEvent (UnityEvent&lt;int&gt;, fires with the new level
    ///    after native level-up rewards already applied) and grants Cash via
    ///    CurrencyManager.AddCurrencyAmount - the same manager method used everywhere else
    ///    currency is granted (CLAUDE.md Hard Rule #4). See BudgetBestieCashPerLevelMultiplier
    ///    for the exact (placeholder) formula.
    ///
    /// 3. The Algorithm Itself - REDESIGNED (was: +60%/lvl global speed passiveBoost,
    ///    confirmed non-functional for the same "live-read, no discrete hook" reason as
    ///    Budget Bestie - production timing is read live and repeatedly from
    ///    BusinessHolder's own timer code). Manager.cs was checked for any native
    ///    "periodic grant while owned" mechanism first - none exists; activeDuration/
    ///    cooldownDuration/activeUpgrade is a player-ACTIVATED temporary-boost system
    ///    (TryActivateManager), not a passive automatic one, so it doesn't fit "owned = the
    ///    effect just runs" the way every other Rare/Legendary manager works. Redesigned
    ///    instead as periodically re-activating the existing AlgorithmPushBoost (2x speed,
    ///    10 min, already built and working) via a timer in Update(), for as long as The
    ///    Algorithm Itself is owned - effectively keeping AlgorithmPushBoost perpetually
    ///    topped up rather than something the player has to trigger. Each tick calls
    ///    BoostsManager.RemoveActiveBoost then SetActiveBoost (both real manager methods)
    ///    rather than the inventory-based ForceBuyItemsForFree/TryUseBoost path, because
    ///    ForceBuyItemsForFree only adds INVENTORY count - confirmed via full read of
    ///    BoostsManager.cs that it does not itself activate anything; SetActiveBoost is
    ///    the actual activation call. Remove-then-set (rather than just repeatedly calling
    ///    SetActiveBoost) guarantees exactly one active instance at a time regardless of
    ///    AlgorithmPushBoost's own IMergableBoost/mergeRepeatedUses behavior, so this can
    ///    never accidentally stack the multiplier from repeated refreshes.
    ///
    /// KNOWN BEHAVIOR CHANGE (Responsibility 3): the redesigned effect reuses the EXISTING
    /// AlgorithmPushBoost asset as-is (fixed 2x speed) rather than a level-scaled variant,
    /// so The Algorithm Itself's effect no longer scales with the manager's own level the
    /// way "+60%/lvl" originally implied - owning it now means "AlgorithmPushBoost is
    /// always active", not "always active AND stronger at higher levels". Deliberate
    /// simplification per this task's own framing (reuse the existing, working Boost);
    /// flagged explicitly per CLAUDE.md Hard Rule #11 rather than silently narrowed.
    ///
    /// 4. The Nepo Baby - REDESIGNED (was: 8 individually-targeted +30%/lvl dropWeight
    ///    passiveBoosts entries, one per other Rare/Legendary manager, confirmed
    ///    non-functional for two independent reasons: dropWeight resolves synchronously
    ///    inside ShopItemHolder.ClaimRewards with no pre-roll event to intercept, AND
    ///    UpgradeEffectResolver.GetMatchingBlocks - the code ModifyDropWeight itself relies
    ///    on - never iterates ManagersManager's owned managers at all, so it structurally
    ///    could never see a Manager-sourced dropWeight passiveBoost regardless of the first
    ///    problem. A non-vendored in-memory DropTable-cloning fix was investigated
    ///    separately (see docs/BugTracker.md) and found technically feasible ONLY in
    ///    isolation - actually making a reweighted clone's odds govern a real purchase would
    ///    require bypassing ShopManager.CompleteExternalPurchase's hardcoded ClaimRewards()
    ///    call, which is not a small pre-roll patch but closer to a parallel purchase-
    ///    completion path. Rejected as too risky/large in scope, same reasoning as avoiding
    ///    a live-read vendored patch for Budget Bestie/Algorithm Itself above. Redesigned
    ///    instead to amplify an existing, already-working mechanism the same way those two
    ///    were: grants a bonus CloutCoin amount on top of whatever the native level-up
    ///    reward already granted, whenever the player levels up while The Nepo Baby is
    ///    owned - "born lucky" flavor via more currency to spend on Capsules, rather than
    ///    directly manipulating pull odds. Hooks the same PlayerStats.OnLevelUpEvent as
    ///    Budget Bestie and grants CloutCoin via CurrencyManager.AddCurrencyAmount. See
    ///    NepoBabyCloutCoinPerLevelMultiplier for the exact (placeholder) formula.
    ///
    /// KNOWN DIFFERENCE FROM BUDGET BESTIE (Responsibility 4): unlike Budget Bestie's Cash
    /// grant, which scales with BOTH the new player level and Budget Bestie's own manager
    /// level, this formula scales with ONLY The Nepo Baby's own manager level per this
    /// task's explicit formula (managerLevel x 5) - a flat per-level-up amount rather than
    /// one that also grows with player level. Deliberate, matching the exact formula
    /// specified rather than silently reusing Budget Bestie's two-factor shape.
    /// </summary>
    [AddComponentMenu("InfluencerRise/Rare-Legendary Manager Effects")]
    public class RareLegendaryManagerEffects : MonoBehaviour
    {
        /// <summary>Cash granted on level-up = newPlayerLevel x BudgetBestie's own manager level x this. Placeholder, not yet balance-tested.</summary>
        private const float BudgetBestieCashPerLevelMultiplier = 10f;

        /// <summary>How often (in real seconds) Responsibility 3 checks whether AlgorithmPushBoost needs refreshing. Placeholder, not yet balance-tested - well under the boost's own 10-minute duration so it never actually lapses.</summary>
        private const float AlgorithmItselfRefreshIntervalSeconds = 60f;

        /// <summary>Duration (seconds) each refresh sets AlgorithmPushBoost's active timer to. Matches AlgorithmPushBoost.asset's own configured duration (10 minutes) - kept as a separate constant since SetActiveBoost requires an explicit duration rather than reading the asset's own.</summary>
        private const float AlgorithmPushBoostDurationSeconds = 600f;

        /// <summary>Bonus CloutCoin granted on level-up = NepoBaby's own manager level x this. Placeholder, not yet balance-tested. Flat per-level-up amount (does not also scale with player level, unlike Budget Bestie's formula - see this file's header).</summary>
        private const float NepoBabyCloutCoinPerLevelMultiplier = 5f;

        [Tooltip("The Growth Hacker manager. While owned, tops up XP grants with the extra amount its level-scaled playerXp multiplier is owed. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager growthHacker;

        [Tooltip("The Budget Bestie manager. While owned, grants a Cash burst on every player level-up, scaled by player level x this manager's own level. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager budgetBestie;

        [Tooltip("The Algorithm Itself manager. While owned, periodically re-activates algorithmPushBoost so it never lapses. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager algorithmItself;

        [Tooltip("The AlgorithmPushBoost asset periodically re-activated by Responsibility 3.")]
        [SerializeField] private Boost algorithmPushBoost;

        [Tooltip("The Cash currency asset granted by Responsibility 2.")]
        [SerializeField] private Currency cash;

        [Tooltip("The Nepo Baby manager. While owned, grants a bonus CloutCoin amount on every player level-up, scaled by this manager's own level. Leave unassigned to disable this responsibility entirely.")]
        [SerializeField] private Manager nepoBaby;

        [Tooltip("The CloutCoin currency asset granted by Responsibility 4.")]
        [SerializeField] private Currency cloutCoin;

        private bool _isApplyingGrowthHackerBonus;
        private float _secondsSinceLastAlgorithmItselfRefresh;

        private void OnEnable()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnXpAdded.AddListener(HandleXpAdded);
                PlayerStats.Instance.OnLevelUpEvent.AddListener(HandleLevelUp);
                PlayerStats.Instance.OnLevelUpEvent.AddListener(HandleLevelUpForNepoBaby);
            }
        }

        private void OnDisable()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnXpAdded.RemoveListener(HandleXpAdded);
                PlayerStats.Instance.OnLevelUpEvent.RemoveListener(HandleLevelUp);
                PlayerStats.Instance.OnLevelUpEvent.RemoveListener(HandleLevelUpForNepoBaby);
            }
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

        /// <summary>Responsibility 2: grants a Cash burst on level-up while The Budget Bestie is owned, scaled by player level x manager level.</summary>
        private void HandleLevelUp(int newPlayerLevel)
        {
            if (budgetBestie == null || cash == null || ManagersManager.Instance == null || CurrencyManager.Instance == null) return;

            ManagerHolder holder = ManagersManager.Instance.GetHolder(budgetBestie);
            if (holder == null || holder.level <= 0) return;

            BigNumber cashGrant = (BigNumber)newPlayerLevel * holder.level * BudgetBestieCashPerLevelMultiplier;
            if (cashGrant <= 0) return;

            CurrencyManager.Instance.AddCurrencyAmount(cash, cashGrant, SourceType.Generic);
        }

        /// <summary>
        /// Responsibility 3: while The Algorithm Itself is owned, periodically re-activates
        /// algorithmPushBoost so it's perpetually active rather than something the player
        /// has to trigger. Removes any currently-active instance before re-setting it, so
        /// this can never stack multiple active copies of the same boost.
        /// </summary>
        private void Update()
        {
            if (algorithmItself == null || algorithmPushBoost == null) return;
            if (ManagersManager.Instance == null || BoostsManager.Instance == null) return;

            _secondsSinceLastAlgorithmItselfRefresh += Time.deltaTime;
            if (_secondsSinceLastAlgorithmItselfRefresh < AlgorithmItselfRefreshIntervalSeconds) return;

            _secondsSinceLastAlgorithmItselfRefresh -= AlgorithmItselfRefreshIntervalSeconds;

            ManagerHolder holder = ManagersManager.Instance.GetHolder(algorithmItself);
            if (holder == null || holder.level <= 0) return;

            BoostsManager.Instance.RemoveActiveBoost(algorithmPushBoost);
            BoostsManager.Instance.SetActiveBoost(algorithmPushBoost, AlgorithmPushBoostDurationSeconds);
        }

        /// <summary>Responsibility 4: grants a bonus CloutCoin amount on top of the native level-up reward while The Nepo Baby is owned, scaled by this manager's own level only.</summary>
        private void HandleLevelUpForNepoBaby(int newPlayerLevel)
        {
            if (nepoBaby == null || cloutCoin == null || ManagersManager.Instance == null || CurrencyManager.Instance == null) return;

            ManagerHolder holder = ManagersManager.Instance.GetHolder(nepoBaby);
            if (holder == null || holder.level <= 0) return;

            BigNumber cloutCoinGrant = (BigNumber)holder.level * NepoBabyCloutCoinPerLevelMultiplier;
            if (cloutCoinGrant <= 0) return;

            CurrencyManager.Instance.AddCurrencyAmount(cloutCoin, cloutCoinGrant, SourceType.Generic);
        }
    }
}
