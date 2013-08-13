using System;
using System.Collections.Generic;
using System.Linq;
using Beer.Interface.World.Triggers;
using Beer.World.Interfaces;
using Psy.Core;

namespace Beer.World.Triggers
{
    public class ZombieSpawnTrigger : TimedTrigger
    {
        public override bool SendToClient { get { return false; } }

        private INewWorld _world;
        private EngineBase _engine;

        public IEnumerable<KeyValuePair<string, string>> Configuration
        {
            get { throw new NotImplementedException(); }
        }

        public ZombieSpawnTrigger(EngineBase engine, INewWorld world, TriggerKey key, Vector location)
            : base(world, key, location, 1)
        {
            _world = world;
            OnActivated += SpawnZombie;
        }

        private void SpawnZombie(ITrigger trigger)
        {
            _engine.SpawnEntityAtRandomObservedLocation("Zombie");
        }
    }
}
