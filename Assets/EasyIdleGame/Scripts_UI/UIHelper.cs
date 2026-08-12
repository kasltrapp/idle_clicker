using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyIdleGame.UI
{
    public static class UIHelper
    {
        public static string FormatTime(double totalSeconds, string formatStyle = "h m s", bool dynamic = true, string decimalFormatting = "F1")
        {
            int totalMilliseconds = (int)(totalSeconds * 1000);

            string[] requestedUnits = formatStyle
                .Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);


            // Full breakdown of time units
            var fullBreakdown = new Dictionary<string, double>();
            long remaining = totalMilliseconds;

            Dictionary<string, long> timeUnits = new Dictionary<string, long>
            {
                { "d", 86400000 }, // days
                { "h", 3600000 },  // hours
                { "m", 60000 },    // minutes
                { "s", 1000 },     // seconds
                { "ms", 1 }        // milliseconds
            };

            foreach (var unit in timeUnits)
            {
                if (requestedUnits.Contains(unit.Key))
                {
                    long val = remaining / unit.Value;
                    if (dynamic && val == 0) continue;

                    fullBreakdown[unit.Key] = val;
                    remaining %= unit.Value;
                }
            }

            if (remaining != 0)
            {
                foreach (var unit in timeUnits.Reverse())
                {
                    if (requestedUnits.Contains(unit.Key))
                    {
                        if (fullBreakdown.ContainsKey(unit.Key)) fullBreakdown[unit.Key] += remaining / (float)unit.Value;
                        else fullBreakdown[unit.Key] = remaining / (double)unit.Value;

                        break;
                    }
                }
            }

            var result = new StringBuilder();

            foreach (var unit in requestedUnits)
            {
                var cleanUnit = unit.ToLower();

                if (!fullBreakdown.ContainsKey(cleanUnit)) continue;

                result.Append($"{fullBreakdown[cleanUnit].ToString(decimalFormatting)}{cleanUnit} ");
            }

            if (result.Length == 0)
            {
                return $"{totalSeconds.ToString(decimalFormatting)}s";
            }

            return result.ToString().TrimEnd();
        }
    }
}