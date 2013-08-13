using System;
using System.Linq;
using Psy.Core;

namespace Vortex.PerformanceHud
{
    public class TimeLine
    {
        public readonly FixedLengthList<double> SamplePoints;
        public string Name { get; set; }
        public int AlertLevelValue { get; set; }
        public int Width { get { return SamplePoints.MaxCount; } }
        public double MaxValue { get; private set; }
        public double MinValue { get; private set; }

        public double LastValue
        {
            get { return SamplePoints.LastOrDefault(); }
        }

        public TimeLine(int numSamplePoints, string name, int alertLevelValue)
        {
            MaxValue = 0;
            MinValue = 0;
            Name = name;
            SamplePoints = new FixedLengthList<double>(numSamplePoints);
            AlertLevelValue = alertLevelValue;
        }

        public double RollingAverage(int values = 4)
        {
            double total = 0;
            var count = SamplePoints.Count;
            var max = Math.Min(values, count);
            for (var i = 0; i < max; i++)
            {
                var index = count - (1 + i);
                total += SamplePoints[index];
            }
            return total/values;
        }

        public void AddSample(double sample)
        {
            SamplePoints.Add(sample);

            MaxValue = SamplePoints.Max();
            MinValue = SamplePoints.Min();
        }
    }
}
