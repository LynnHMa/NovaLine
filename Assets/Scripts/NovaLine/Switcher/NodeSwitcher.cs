using NovaLine.Element;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class NodeSwitcher : NovaSwitcher
    {
        public Condition switchCondition = Condition.DEFAULT_CONDITION;

        public NodeSwitcher() { 
            guid = Guid.NewGuid().ToString();
        }
        public NodeSwitcher(Condition switchCondition, Node inputNode,Node outputNode,string guid)
        {
            this.switchCondition = switchCondition;
            outputElement = inputNode;
            inputElement = outputNode;
            this.guid = guid;
        }
    }
}
