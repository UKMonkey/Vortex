using System.Collections.Generic;

namespace Vortex.PerformanceHud
{
    public delegate void OnChange(string groupName, string barName);

    public class BarChart
    {
        public Dictionary<string, BarGroup> BarGroups { get; set; }

        public event OnChange OnChange;

        public BarChart()
        {
            BarGroups = new Dictionary<string, BarGroup>(2);
        }

        public void AddSample(string groupName, string barName, IBarChartValue value)
        {
            if (!BarGroups.ContainsKey(groupName))
            {
                BarGroups[groupName] = new BarGroup();
            }
            BarGroups[groupName].AddSample(barName, value);

            if (OnChange != null)
                OnChange(groupName, barName);
        }

        public void Tick()
        {
            foreach (var barGroup in BarGroups)
            {
                barGroup.Value.Tick();
            }
        }

        public void Clear()
        {
            BarGroups.Clear();
        }
    }
}