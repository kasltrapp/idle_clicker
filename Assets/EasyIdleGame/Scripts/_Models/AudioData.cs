using UnityEngine;

namespace EasyIdleGame
{
    public enum AudioCategory
    {
        SFX,
        BackgroundMusic,
        UI
    }

    [System.Serializable]
    public class AudioData
    {
        [Tooltip("The audio clip to play.")]
        public AudioClip clip;

        [Range(0f, 1f)]
        [Tooltip("Volume scale for this specific sound.")]
        public float volume = 1f;

        [Tooltip("The category of this audio, used for muting and routing.")]
        public AudioCategory category = AudioCategory.SFX;

        public bool IsValid() => clip != null;
    }
}
