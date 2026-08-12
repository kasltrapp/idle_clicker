using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public class BusinessDetailDisplayer : BaseDisplayer, IOpenable
    {
        public IOpenable iOpenable => this;

        // IOpenable
        public GameObject GameObject => gameObject;
        public AnimationClip OpenAnimation => openAnimation;
        public MonoBehaviour MonoBehaviour => this;
        public AnimationClip CloseAnimation => closeAnimation;
        public Business business;
        public ManagerDisplayer mDisplayer;
        public Image image;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text nameText;
        public TMP_Text description;

        [Space(InspectorSettings.INSPECTOR_SPACE)]
        public TMP_Text production;
        public TMP_Text cost;
        public TMP_Text lockText;

        [Header("Animations")]
        public AnimationClip openAnimation;
        public AnimationClip closeAnimation;

        public override void Initialize()
        {
            mDisplayer.autoRedraw = false;
        }

        public void OpenAndRedraw(Business _business)
        {
            business = _business;
            iOpenable.Open();
            Redraw();
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();
            if (business == null) return;

            UpdateText(nameText, business.GetDisplayName());
            UpdateImage(image, business.Metadata.icon);
            UpdateText(description, business.Metadata.description);

            try // when currencies manager is not asssigned yet or loaded
            {
                UpdateText(cost, "Cost: " + InputsToString(business.cost));
                UpdateText(lockText, "Locks: " + LocksToString(business.locks));
            }
            catch { }

            if (business.outputs.Count > 0)
                UpdateText(production, business.outputs[0].currencyOutput ? $"Produces {business.outputs[0].currencyOutput.GetIconString()}" : $"Produces {business.outputs[0].businessOutput.GetDisplayName()}");

        }

        public override void Redraw_Default_Empty()
        {
            base.Redraw_Default_Empty();

            UpdateText(nameText, "BusinessName");
            UpdateText(description, "BusinessDescription");

            UpdateText(cost, "Cost: 1$");
            UpdateText(lockText, "Locks: lvl 1");
        }

        public override void Redraw()
        {
            base.Redraw();
            if (business == null) return;

            BusinessHolder businessHolder = BusinessesManager.Instance.GetHolder(business);
            if (businessHolder != null)
            {
                UpdateText(cost, $"Cost: {InputsToString(business.cost)} ({base.InputsToString(businessHolder.iBuyable.GetRealCostToInputs(1))})");
            }

            mDisplayer.gameObject.SetActive(business.manager);
            if (!business.manager) return;

            mDisplayer.manager = business.manager;
            mDisplayer.Redraw();
        }

        // binding
        public void CloseDisplayer() => iOpenable.Close();
    }
}
