using TMPro;
using UnityEngine;

namespace EasyIdleGame.UI
{
    public class LevelUpDisplayer : BaseDisplayer, IOpenable
    {
        public IOpenable iOpenable => this;

        // IOpenable
        public GameObject GameObject => gameObject;
        public AnimationClip OpenAnimation => openAnimation;
        public MonoBehaviour MonoBehaviour => this;
        public AnimationClip CloseAnimation => closeAnimation;

        public TMP_Text levelText;

        [Header("Animations")]
        public AnimationClip openAnimation;
        public AnimationClip closeAnimation;

        public void OnLevelUp(int level)
        {
            iOpenable.Open();
            Redraw(level);
        }

        public void Redraw(int level)
        {
            UpdateText(levelText, $"{level}");
        }

        public void CloseDisplayer() => iOpenable.Close();
    }
}