using TMPro;
using EasyIdleGame;
using EasyIdleGame.UI;

namespace InfluencerRise.UI
{
    /// <summary>
    /// Adds the fields the base BusinessDetailDisplayer doesn't expose (speed, cost multiplier)
    /// on top of what it already covers natively (name, description via Metadata.description,
    /// cost, production, and the assigned Manager's name/level/owned-status via mDisplayer).
    /// </summary>
    public sealed class BusinessDetailPopup : BusinessDetailDisplayer
    {
        public TMP_Text speedText;
        public TMP_Text costMultiplierText;

        public override void Redraw()
        {
            base.Redraw();
            if (business == null) return;

            BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
            float actualSeconds = holder != null
                ? holder.iProducable.GetActualTimeToProduce()
                : business.InitialTimeToProduce;

            UpdateText(speedText, $"Speed: {UIHelper.FormatTime(actualSeconds)} per round");
            UpdateText(costMultiplierText, $"Cost multiplier: x{business.costMultiplier:0.00} per purchase");

            // Base Redraw_Default() checks business.outputs (the deprecated legacy list, always
            // empty for content authored against outputTables like ours) instead of the real
            // business.Outputs property (outputTables flattened) - confirmed via Business.cs. Fix
            // it here rather than relying on the base implementation.
            var realOutputs = business.Outputs;
            if (realOutputs != null && realOutputs.Count > 0)
            {
                var first = realOutputs[0];
                string producesText = first.currencyOutput != null
                    ? $"Produces {first.currencyOutput.GetIconString()}"
                    : (first.businessOutput != null ? $"Produces {first.businessOutput.GetDisplayName()}" : "Produces -");
                UpdateText(production, producesText);
            }
        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();
            UpdateText(speedText, "Speed: 0s per round");
            UpdateText(costMultiplierText, "Cost multiplier: x1.00 per purchase");
        }
    }
}
