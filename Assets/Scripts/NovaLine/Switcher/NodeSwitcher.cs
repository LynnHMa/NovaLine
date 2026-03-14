using NovaLine.Element;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class NodeSwitcher : NovaSwitcher
    {
        public Condition switchCondition;

        public NodeSwitcher() { 
            guid = Guid.NewGuid().ToString();
            switchCondition = new(outputElement);
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
