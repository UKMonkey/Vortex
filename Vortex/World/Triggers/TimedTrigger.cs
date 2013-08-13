using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beer.Interface.World.Triggers;
using Beer.World.Interfaces;
using Psy.Core;

namespace Beer.World.Triggers
{
    public class TimedTrigger : TriggerBase
    {
        public override TriggerActivationMethod ActivationMethod 
            { get { return TriggerActivationMethod.Timer; }}

        protected uint Frequency 
            { get; private set; }

        protected uint LastActivated { get; private set; }
        protected uint NextActivation { get; private set; }

        private readonly ITimeOfDayProvider _timeProvider;

        protected TimedTrigger(ITimeOfDayProvider timeProvider, TriggerKey key, Vector location, uint frequency)
            :base (key, location)
        {
            Frequency = frequency;
            _timeProvider = timeProvider;
            LastActivated = 0;
            NextActivation = frequency;
        }

        public override void Update()
        {
            //now - how to catch loops ... hmmmmm
            // lastActivated < time < nextActivation (usually)
            // lastActivated < time >= nextActivation (on activation)
            // lastActivated > time < nextActivation (on loop)
            // lastActivated > time >= nextActivation (on loop & activation)
            if (_timeProvider.TimeOfDay >= NextActivation)
            {
                RegisterActivation();
                OnActivated(this);
            }
        }

        private void RegisterActivation()
        {
            LastActivated = _timeProvider.TimeOfDay;
            NextActivation = (_timeProvider.TimeOfDay + Frequency) & _timeProvider.TicksPerDay;
        }
    }
}
