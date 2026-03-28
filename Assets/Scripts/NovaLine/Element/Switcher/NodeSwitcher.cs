using System;

namespace NovaLine.Element.Switcher
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
        public override NovaElement copy()
        {
            var clone = base.copy() as NodeSwitcher;
            if (clone == null) return null;
            
            clone.switchCondition = (Condition)switchCondition.copy();
            clone.switchCondition.parent = clone;
            
            return clone;
        }
    }
}
