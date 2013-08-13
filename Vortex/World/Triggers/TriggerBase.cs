using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beer.Interface.World.Triggers;
using Psy.Core;

namespace Beer.World.Triggers
{
    public abstract class TriggerBase : ITrigger
    {
        public TriggerActivated OnActivated { get; set; }

        private readonly TriggerKey _key;
        public virtual TriggerKey Key { get { return _key; } }

        private readonly Vector _location;
        public virtual Vector Location { get { return _location; } }

        public virtual TriggerActivationMethod ActivationMethod { get { throw new NotImplementedException(); } }
        public virtual List<KeyValuePair<string, string>> Configuration { get { throw new NotImplementedException(); } }
        public virtual bool SendToClient { get { throw new NotImplementedException(); } }


        protected TriggerBase(TriggerKey key, Vector location)
        {
            _key = key;
            _location = location;
        }

        public abstract void Update();
    }
}
