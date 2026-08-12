using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public abstract class BaseDisplayer : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnValidate() => Redraw_Default();
#endif

        [Tooltip("If true, the Redraw method will be called every frame")]
        public bool autoRedraw = true;

        private void Start() => Initialize();

        private void Update()
        {
            if (!autoRedraw) return;
            Redraw();
        }

        public virtual void Initialize() { }

        public virtual void Redraw() => Redraw_Default();

        /// <summary>
        /// This method is meant to be used to set the default values of the UI elements, this method is meant to use the displayed object (such as business) to set the values
        /// (called in editor)
        /// </summary>
        public virtual void Redraw_Default() => Redraw_Default_Empty();

        /// <summary>
        /// This method is meant to be used to set the default values of the UI elements when the displayed object (such as business) is null 
        /// (called in editor)
        /// </summary>
        public virtual void Redraw_Default_Empty() { }

        protected void EnableComponent(GameObject c, bool e)
        {
            if (c == null) return;
            c.SetActive(e);
        }

        protected void EnableComponent(ButtonHolder c, bool e)
        {
            if (c == null || c.button == null) return;
            c.button.gameObject.SetActive(e);
        }

        protected string ColorfulText(string text, bool green) => EconomyTextFormatter.ColorfulText(text, green);

        protected string InputsToString(List<Input> inputs, bool colorful = true)
        {
            List<string> costs = new();

            foreach (Input input in inputs)
            {
                if (input.businessInput != null) costs.Add(BusinessInputToString(input, colorful));

                if (input.currencyInput != null) costs.Add(CurrencyInputToString(input, colorful));
            }

            return string.Join(" ", costs);
        }

        protected string LocksToString(List<Lock> locks, bool colorful = true)
        {
            List<string> lock_ = new();

            foreach (Lock input in locks)
            {
                if (input.businessLock != null) lock_.Add(BusinessLockToString(input, colorful));

                if (input.currencyLock != null) lock_.Add(CurrencyLockToString(input, colorful));

                if (input.playerLevelLock) lock_.Add(LevelRequirementToString(input, colorful));
            }

            return string.Join(" ", lock_);
        }

        protected string UpgradeBlocksToString(List<UpgradeBlock> upgrades)
        {
            List<string> upgrade = new();

            foreach (UpgradeBlock u in upgrades)
            {
                upgrade.Add(UpgradeBlockToString(u));
            }

            return string.Join(" ", upgrade);
        }

        protected virtual string UpgradeBlockToString(UpgradeBlock u)
        {
            return EconomyTextFormatter.UpgradeBlockToString(u);
        }

        // override this method to change the format of the requirement string
        protected virtual string BusinessInputToString(Input bInput, bool colorful = true)
        {
            return EconomyTextFormatter.BusinessInputToString(bInput, colorful);
        }

        protected virtual string CurrencyInputToString(Input cInput, bool colorful = true)
        {
            return EconomyTextFormatter.CurrencyInputToString(cInput, colorful);
        }

        protected virtual string BusinessLockToString(Lock bLock, bool colorful = true)
        {
            return EconomyTextFormatter.BusinessLockToString(bLock, colorful);
        }

        protected virtual string CurrencyLockToString(Lock cLock, bool colorful = true)
        {
            return EconomyTextFormatter.CurrencyLockToString(cLock, colorful);
        }

        protected virtual string LevelRequirementToString(Lock lLock, bool colorful = true)
        {
            return EconomyTextFormatter.LevelRequirementToString(lLock, colorful);
        }

        // ------------ Audio ------------
        protected virtual void PlayAudio(AudioClip clip)
        {
            AudioManager aManager = FindObjectOfType<AudioManager>();
            if (aManager == null) return;

            if (clip == null) return;
            aManager.PlaySFX(clip);
        }

        // ------------ UI Updater ------------
        protected virtual void UpdateText(TMP_Text t, string v) => UIUpdater.UpdateText(t, v);
        protected virtual void UpdateText(ButtonHolder t, string v) => UIUpdater.UpdateText(t, v);
        protected virtual void SetButtonInteractable(Button b, bool v) => UIUpdater.SetButtonInteractable(b, v);
        protected virtual void SetButtonInteractable(ButtonHolder b, bool v) => UIUpdater.SetButtonInteractable(b, v);
        protected virtual void UpdateImage(Image i, Sprite s) => UIUpdater.UpdateImage(i, s);
        protected virtual void UpdateImageColor(Image i, Color color) => UIUpdater.UpdateImageColor(i, color);
        protected virtual void SetSliderValues(Slider s, float min, float max, double value) => UIUpdater.SetSliderValues(s, min, max, (float)value);
        protected virtual void SetSliderValues(Slider s, float min, float max, float value) => UIUpdater.SetSliderValues(s, min, max, value);
        protected virtual void UpdateImageOpacity(Image i, float opacity) => UIUpdater.UpdateImageOpacity(i, opacity);
        protected virtual Color ChangeColorOpacity(Color c, float opacity) => UIUpdater.ChangeColorOpacity(c, opacity);
        protected virtual void UpdateTextOpacity(TMP_Text t, float opacity) => UIUpdater.UpdateTextOpacity(t, opacity);

        // ------------ Animations ------------
        private Animator _animator;

        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                    if (_animator == null)
                    {
                        Debug.LogWarning("Animator not found on " + gameObject.name + ". Please assign an Animator component.");
                    }
                }

                return _animator;
            }
        }

        protected void PlayAnimation(AnimationClip animationName)
        {
            if (Animator == null) return;
            Animator.Play(animationName.name);
        }
    }
}
