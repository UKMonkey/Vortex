using System.Linq;
using Psy.Core;

namespace Vortex.PerformanceHud
{
    public class Bar
    {
        public const int MaxSamples = 5;

        public decimal Value { get { return _values.Count == 0 ? 0 : _values.Average(v => v); }}
        private readonly FixedLengthList<decimal> _values;
        private decimal _accumulator;

        public Bar()
        {
            _values = new FixedLengthList<decimal>(MaxSamples);
        }

        public void AddSample(IBarChartValue value)
        {
            _accumulator += value.BarValue;
        }

        public void Tick()
        {
            _values.Add(_accumulator);
            _accumulator = 0;
        }
    }
}