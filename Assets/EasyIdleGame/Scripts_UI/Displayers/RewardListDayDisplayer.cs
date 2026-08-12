using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public class RewardListDayDisplayer : BaseDisplayer
    {
        public Output reward;
        public int dayNumber;

        // whether or not will this reward be claimable today
        public bool toBeClaimed;
        public bool isClaimed;
        public bool isMissed;
        public Image icon;
        public TMP_Text dayText;
        public TMP_Text rewardAmountText;
        public GameObject claimedOverlay;
        public GameObject missedOverlay;

        public void UpdateDataAndRedraw(Output reward, int dayNumber, bool toBeClaimed, bool isClaimed, bool isMissed)
        {
            this.reward = reward;
            this.dayNumber = dayNumber;
            this.toBeClaimed = toBeClaimed;
            this.isClaimed = isClaimed;
            this.isMissed = isMissed;
            Redraw();
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();

            if (reward == null) return;

            Sprite img = reward.currencyOutput != null ? reward.currencyOutput.Metadata.icon : reward.businessOutput != null ? reward.businessOutput.Metadata.icon : null;

            UpdateImage(icon, img);
            UpdateText(rewardAmountText, $"x{reward.outputAmount}");

            UpdateText(dayText, $"Day {dayNumber}");
            UpdateImageOpacity(icon, toBeClaimed ? 1f : .5f);
            UpdateTextOpacity(dayText, toBeClaimed ? 1f : .5f);
            UpdateTextOpacity(rewardAmountText, toBeClaimed ? 1f : .5f);
            EnableComponent(claimedOverlay, isClaimed);
            EnableComponent(missedOverlay, isMissed);
        }

    }
}
