using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public enum ButtonAudioType
    {
        Primary,
        Secondary,
        Custom
    }

    [RequireComponent(typeof(Button))]
    public class AudioButton : BaseDisplayer
    {
        public ButtonAudioType buttonAudioType;

        [Tooltip("Audio clip to play when the button is clicked. " +
            "If assigned, this overrides buttonAudioType; leave null to use the selected shared sound."
        )]
        public AudioClip customAudioClip;

        private void Start() => Initialize();

        public override void Initialize()
        {
            GetComponent<Button>().onClick.AddListener(PlaySFX);
        }

        private void PlaySFX()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager not found");
                return;
            }

            if (customAudioClip != null)
            {
                AudioManager.Instance.PlaySFX(customAudioClip);
                return;
            }

            switch (buttonAudioType)
            {
                case ButtonAudioType.Primary:
                    AudioManager.Instance.PlayDefaultButtonSFX();
                    break;
                case ButtonAudioType.Secondary:
                    AudioManager.Instance.PlaySecondaryButtonSFX();
                    break;
            }
        }
    }
}
