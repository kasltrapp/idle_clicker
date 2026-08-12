using System;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame.UI
{
    public class AudioManager : ManagerBase<AudioManager>
    {
        [CommentArea("Audio Manager", "Central audio component for background music and shared button sound effects.", "Place one AudioManager in the scene, assign backgroundSource and sfxSource, then add AudioButton to UI buttons that should play the default or secondary click sounds.")]
        [SerializeField] private string _audioManagerComment;

        [Tooltip("Music clip played on Start through backgroundSource. Leave null for no automatic background music.")]
        public AudioClip backgroundMusic;

        [Tooltip("Default click sound used by Primary AudioButton instances. Leave null to make primary buttons silent.")]
        public AudioClip defaultButtonSFX;

        [Tooltip("Alternate click sound used by Secondary AudioButton instances. Leave null to make secondary buttons silent.")]
        public AudioClip secondaryButtonSFX;

        [Header("Settings")]
        [Tooltip("If enabled, audio events triggered by an item will only play if the item belongs to the currently active location or is global.")]
        public bool playSoundsForCurrentLocationOnly = false;

        [Header("Sources")]
        [Tooltip("AudioSource used for looping background music. Must be assigned for background playback and mute state.")]
        public AudioSource backgroundSource;

        [Tooltip("AudioSource used for one-shot sound effects. Must be assigned for button sounds and SFX mute state.")]
        public AudioSource sfxSource;

        [HideInInspector]
        public bool[] mutedCategories = new bool[System.Enum.GetValues(typeof(AudioCategory)).Length];

        public bool IsSFXMuted => sfxSource.mute;
        public bool IsBackgroundMuted => backgroundSource.mute;

        public bool IsCategoryMuted(AudioCategory category)
        {
            if ((int)category < mutedCategories.Length)
            {
                return mutedCategories[(int)category];
            }
            return false;
        }

        public void MuteCategory(AudioCategory category, bool mute)
        {
            if ((int)category < mutedCategories.Length)
            {
                if (mutedCategories[(int)category] == mute) return;
                mutedCategories[(int)category] = mute;

                if (category == AudioCategory.BackgroundMusic)
                {
                    MuteBackground(mute);
                }
                else if (category == AudioCategory.SFX)
                {
                    MuteSFX(mute);
                }

                OnSettingsChanged?.Invoke();
            }
        }

        [Tooltip("Invoked when background or SFX mute state changes, so UI toggles can refresh their visual state.")]
        public UnityEvent OnSettingsChanged = new UnityEvent();

        public void Start()
        {
            if (backgroundMusic != null)
                PlayBackground(backgroundMusic);

            if (BusinessesManager.Instance)
            {
                BusinessesManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                BusinessesManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
                BusinessesManager.Instance.OnHolderLeveledUpEvent.AddListener(holder => PlayAudio(holder.Item.LevelUpSound, holder.Item));
                BusinessesManager.Instance.OnHolderProductionStartedEvent.AddListener(holder => PlayAudio(holder.Item.ProductionStartSound, holder.Item));
                BusinessesManager.Instance.OnHolderProductionFinishedEvent.AddListener(holder => PlayAudio(holder.Item.ProductionEndSound, holder.Item));
                BusinessesManager.Instance.OnBusinessesMerged.AddListener(recipe => PlayAudio(recipe.mergeSound, recipe));
            }

            if (UpgradesManager.Instance)
            {
                UpgradesManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                UpgradesManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
            }

            if (ShopManager.Instance)
            {
                ShopManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                ShopManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
            }

            if (LocationsManager.Instance)
            {
                LocationsManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                LocationsManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
                LocationsManager.Instance.OnLocationChanged.AddListener(UpdateLocationBackgroundMusic);

                if (LocationsManager.Instance.ActiveLocation != null)
                {
                    UpdateLocationBackgroundMusic(LocationsManager.Instance.ActiveLocation);
                }
            }

            if (ManagersManager.Instance)
            {
                ManagersManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                ManagersManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
            }

            if (BoostsManager.Instance)
            {
                BoostsManager.Instance.OnItemBought.AddListener(item => PlayAudio(item.BuySound, item));
                BoostsManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
                BoostsManager.Instance.OnBoostUsed.AddListener(item => PlayAudio(item.activateSound, item));
            }

            if (AchievementsManager.Instance)
            {
                AchievementsManager.Instance.OnItemUnlocked.AddListener(item => PlayAudio(item.UnlockSound, item));
                AchievementsManager.Instance.OnAchievementClaimed.AddListener(item => PlayAudio(item.claimSound, item));
            }

            if (PrestigeManager.Instance)
            {
                PrestigeManager.Instance.OnPrestige.AddListener(() => PlayAudio(PrestigeManager.Instance.prestigeSound));
            }

            if (DailyRewardsManager.Instance)
            {
                DailyRewardsManager.Instance.OnRewardClaimed.AddListener(reward => PlayAudio(reward.claimSound, reward));
            }

            if (CurrencyManager.Instance)
            {
                CurrencyManager.Instance.OnCurrencyAmountAddedEvent.AddListener((currency, amount) => PlayAudio(currency.collectSound, currency));
            }
        }

        public void PlayDefaultButtonSFX() => PlaySFX(defaultButtonSFX);

        public void PlaySecondaryButtonSFX() => PlaySFX(secondaryButtonSFX);

        public void ToggleMuteBackground() => MuteBackground(!backgroundSource.mute);

        public void ToggleMuteSFX() => MuteSFX(!sfxSource.mute);

        public void MuteBackground(bool mute)
        {
            if (backgroundSource.mute == mute) return;
            backgroundSource.mute = mute;
            mutedCategories[(int)AudioCategory.BackgroundMusic] = mute;
            OnSettingsChanged?.Invoke();
        }

        public void MuteSFX(bool mute)
        {
            if (sfxSource.mute == mute) return;
            sfxSource.mute = mute;
            mutedCategories[(int)AudioCategory.SFX] = mute;
            OnSettingsChanged?.Invoke();
        }

        public void PlayBackground(AudioClip clip)
        {
            if (clip == null) return;
            backgroundSource.clip = clip;
            backgroundSource.Play();
            backgroundSource.loop = true;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || IsCategoryMuted(AudioCategory.SFX)) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayAudio(AudioData audioData, object sourceItem = null)
        {
            if (audioData == null || !audioData.IsValid()) return;

            if (playSoundsForCurrentLocationOnly && sourceItem is ILocatable locatable)
            {
                if (LocationsManager.Instance != null && LocationsManager.Instance.ActiveLocation != null)
                {
                    if (locatable.Location != null && locatable.Location != LocationsManager.Instance.ActiveLocation)
                    {
                        return; // Skip playing because it belongs to another location
                    }
                }
            }

            if (audioData.category == AudioCategory.BackgroundMusic)
            {
                backgroundSource.clip = audioData.clip;
                backgroundSource.volume = audioData.volume;
                backgroundSource.Play();
                backgroundSource.loop = true;
            }
            else
            {
                if (IsCategoryMuted(audioData.category)) return;
                sfxSource.PlayOneShot(audioData.clip, audioData.volume);
            }
        }

        public void UpdateLocationBackgroundMusic(Location location)
        {
            if (location != null && location.backgroundMusic != null && location.backgroundMusic.IsValid())
            {
                PlayAudio(location.backgroundMusic);
            }
            else
            {
                if (backgroundMusic != null)
                {
                    PlayBackground(backgroundMusic);
                }
                else
                {
                    backgroundSource.Stop();
                }
            }
        }
    }
}
