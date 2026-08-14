using System.Collections.Generic;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Managers
{
    /// <summary>
    /// Bridges a confirmed native gap: a Manager's own "+X%/lvl production, targeting a
    /// BusinessGroup (or everywhere, for an empty filter)" passiveBoosts entry never
    /// actually reaches production. Confirmed via a full-project manager audit (see
    /// docs/BugTracker.md and CLAUDE.md Watch-list): the only native code that ever reads
    /// a Manager's passiveBoosts for business production is
    /// ManagersManager.GetPassiveMultiplierOfType/GetActiveMultiplierOfType, and BOTH of
    /// those only ever look up the ONE Manager assigned as a Business's own dedicated
    /// `.manager` field. A Manager whose passiveBoosts are meant to apply across a whole
    /// BusinessGroup (all 6 Rare managers) or globally (Legendary managers with empty
    /// filters) is never that specific Business's `.manager`, so it is never looked up at
    /// all - confirmed live in Play Mode (owning 5 levels of a Rare manager left the
    /// target business's computed production multiplier completely unchanged).
    ///
    /// This asset plugs into Easy Idle Game's OWN production-output-modifier extension
    /// point instead of touching any vendored file: register one instance of this asset on
    /// a ProductionModifierManager component (see GameManagers), and it will be invoked by
    /// the real production pipeline (ProductionOutputCalculator -&gt; ProductionModifierRegistry
    /// -&gt; here) every time a business produces, for every business.
    ///
    /// WHY THIS ONLY COVERS UpgradeType.production (not speed, purchaseCost, playerXp, or
    /// dropWeight): IProductionOutputModifier only gets a chance to rewrite the *output
    /// amounts* of a production event. Speed affects production *timing* (read live and
    /// repeatedly from BusinessHolder's own timer code, with no equivalent modifier
    /// registry), purchaseCost/playerXp/dropWeight aren't part of a production event at
    /// all. Those effect types needed a different approach or were found to have no clean
    /// native hook - see RareLegendaryManagerEffects.cs and the audit write-up for the
    /// managers this asset does NOT cover.
    ///
    /// HOW IT WORKS: for every owned Manager (level >= 1), read its already-correctly-level-
    /// scaled passiveBoosts (ManagerHolder.CurrentPassiveUpgrades - this part of the native
    /// engine IS correct, it just never gets called for the right business). For every
    /// UpgradeType.production block in that list, check whether the currently-producing
    /// business belongs to the block's own targetBusinessGroups (empty = applies to every
    /// business, exactly like a global Legendary passiveBoost is supposed to), using the
    /// same Business.HasGroupMatch the native engine uses for Upgrades/Boosts. Skip a
    /// manager for its OWN dedicated business - that specific case already works correctly
    /// through the native single-manager lookup, so applying it again here would double-count it.
    /// This is intentionally generic: any future Rare/Legendary manager with a
    /// production-type passiveBoosts entry is covered automatically, no per-manager entry
    /// needs to be added here.
    /// </summary>
    [CreateAssetMenu(fileName = "RareLegendaryManagerProductionModifier", menuName = "InfluencerRise/Managers/Rare-Legendary Manager Production Modifier")]
    public class RareLegendaryManagerProductionModifier : ProductionModifierAsset, IProductionOutputModifier
    {
        public bool AppliesTo(ProductionOutputContext context)
        {
            return context?.ProducingBusiness != null && ManagersManager.Instance != null;
        }

        public void ModifyOutputs(ProductionOutputContext context, List<Output> outputs)
        {
            if (outputs == null || outputs.Count == 0) return;

            Business producingBusiness = context.ProducingBusiness;
            BigNumber combinedMultiplier = 1;

            foreach (ManagerHolder holder in ManagersManager.Instance.holders)
            {
                if (holder == null || holder.level <= 0 || holder.manager == null) continue;
                if (producingBusiness.manager == holder.manager) continue; // already handled natively for its own business

                List<UpgradeBlock> scaledBoosts = holder.CurrentPassiveUpgrades;
                if (scaledBoosts == null) continue;

                foreach (UpgradeBlock block in scaledBoosts)
                {
                    if (block == null || block.upgradeType != UpgradeType.production) continue;

                    List<BusinessGroup> targetGroups = block.filters?.targetBusinessGroups;
                    if (!producingBusiness.HasGroupMatch(targetGroups, "")) continue;

                    combinedMultiplier *= (BigNumber)block.value;
                }
            }

            if (combinedMultiplier == 1) return;

            foreach (Output output in outputs)
                output.MultiplyInPlace(combinedMultiplier);
        }
    }
}
