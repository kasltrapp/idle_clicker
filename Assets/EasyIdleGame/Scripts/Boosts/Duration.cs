using System;

namespace EasyIdleGame
{
    /// <summary>
    /// Class meant to store duration in days, hours, minutes and seconds
    /// </summary>
    [Serializable]
    public class Duration
    {
        public int days;
        public int hours;
        public int minutes;
        public int seconds;

        public TimeSpan TimeSpanDuration => new(days, hours, minutes, seconds);

        public float TotalSeconds => (float)TimeSpanDuration.TotalSeconds;

        public Duration(int days, int hours, int minutes, int seconds)
        {
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public Duration FromSeconds(int seconds) => new(0, 0, 0, seconds);
        public Duration FromMinutes(int minutes) => new(0, 0, minutes, 0);
        public Duration FromHours(int hours) => new(0, hours, 0, 0);
        public Duration FromDays(int days) => new(days, 0, 0, 0);

        public override string ToString()
        {
            return TimeSpanDuration.ToString();
        }
    }
}