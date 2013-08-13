using System.Collections.Generic;
using System.Linq;

namespace Vortex.PerformanceHud
{
    public class BarGroup
    {
        public Dictionary<string, Bar> Bars { get; set; }
        public decimal Peak { get { return Bars.Max(b => b.Value).Value; } }

        public BarGroup()
        {
            Bars = new Dictionary<string, Bar>();
        }

        /// <summary>
        /// Adds a sample to a bar, creates a bar if it doesn't
        /// exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddSample(string name, IBarChartValue value)
        {
            if (!Bars.ContainsKey(name))
            {
                Bars[name] = new Bar();
            }
            Bars[name].AddSample(value);
        }

        public void Tick()
        {
            foreach (var bar in Bars)
            {
                bar.Value.Tick();
            }
        }
    }
}