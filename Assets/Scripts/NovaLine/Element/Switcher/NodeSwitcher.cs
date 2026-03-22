using NovaLine.Element;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class NodeSwitcher : NovaSwitcher
    {
        public Condition switchCondition;

        public NodeSwitcher() : base() { 
            switchCondition = new(this);
        }
        public override string getTypeName()
        {
            return "[Node Edge]";
        }
    }
}
