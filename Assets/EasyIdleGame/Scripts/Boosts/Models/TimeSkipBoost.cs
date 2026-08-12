using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Scriptable object that stores time skip boost data
    /// </summary>
    [CreateAssetMenu(fileName = "TimeSkipBoost", menuName = "EasyIdleGame/Boosts/Time Skip Boost")]
    public class TimeSkipBoost : Boost
    {
        [CommentArea("Time Skip", "Immediately simulates a block of offline production time when used. The boost is consumed, profit is calculated, and BoostsManager.OnTimeSkip is invoked.", "Use this for items like 'Skip 1 Hour'. Set skipTime to 1 hour and enable simulateBoosts if active boost timers should advance during the skipped time.")]
        [SerializeField] private string _timeSkipComment;

        [Tooltip("If enabled, active boost timers advance during the skipped time. Disable if time skips should grant production without consuming active boost duration.")]
        public bool simulateBoosts = false;

        [Tooltip("Amount of offline production time to simulate instantly when this boost is activated.")]
        public Duration skipTime;

        public override ActiveBoostHolder Activate()
        {
            ProfitData data = ProfitCalculator.CalculateProfit(skipTime.TimeSpanDuration, simulateBoosts);
            ProfitCalculator.ApplyProfit(data);

            BoostsManager.Instance.OnTimeSkip.Invoke(data);

            return null;
        }
    }
}
