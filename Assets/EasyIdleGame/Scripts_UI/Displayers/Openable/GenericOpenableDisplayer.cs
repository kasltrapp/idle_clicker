using UnityEngine;

namespace EasyIdleGame.UI
{
    public class GenericOpenableDisplayer : BaseDisplayer, IOpenable
    {
        public IOpenable iOpenable => this;

        // IOpenable
        public GameObject GameObject => gameObject;
        public MonoBehaviour MonoBehaviour => this;
        public AnimationClip OpenAnimation => openAnimation;
        public AnimationClip CloseAnimation => closeAnimation;

        public AnimationClip openAnimation;
        public AnimationClip closeAnimation;

        public bool redrawChildDisplayers = true; // on open

        public void OpenDisplayer()
        {
            iOpenable.Open();
            Redraw();

            if (redrawChildDisplayers)
            {
                BaseDisplayer[] childDisplayers = GetComponentsInChildren<BaseDisplayer>(true);
                foreach (BaseDisplayer displayer in childDisplayers)
                {
                    displayer.Redraw();
                }
            }
        }

        public void CloseDisplayer()
        {
            iOpenable.Close();
        }
    }
}