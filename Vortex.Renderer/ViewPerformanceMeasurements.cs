using System;
using System.Collections.Generic;
using Psy.Core;
using Psy.Core.Console;

namespace Vortex.Renderer
{
    public class ViewPerformanceMeasurements
    {
        public TimeSample WorldRender
        {
            get { return Stats[MeasurementType.WorldRender]; }
        }

        public TimeSample NameplateRender
        {
            get { return Stats[MeasurementType.NameplateRender]; }
        }

        public TimeSample RainRender
        {
            get { return Stats[MeasurementType.RainRender]; }
        }

        public TimeSample RainUpdate
        {
            get { return Stats[MeasurementType.RainUpdate]; }
        }

        public Dictionary<MeasurementType, TimeSample> Stats;

        public ViewPerformanceMeasurements()
        {
            Stats = new Dictionary<MeasurementType, TimeSample>();
            foreach (var rawEnumValue in Enum.GetValues((typeof(MeasurementType))))
            {
                var enumValue = (MeasurementType) rawEnumValue;
                Stats[enumValue] = new TimeSample();
            }
        }

        public void WriteToConsole()
        {
            var console = StaticConsole.Console;

            foreach (var stat in Stats)
            {
                console.AddLine(
                    string.Format("{0}: {1}", stat.Key, stat.Value), Colours.Orange);
            }
        }
    }
}