using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vortex.Interface.World.Triggers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TriggerTypeAttribute : Attribute
    {
        public string TypeName;
    }
}
