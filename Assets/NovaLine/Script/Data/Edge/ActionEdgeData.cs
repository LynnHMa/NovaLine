using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class ActionEdgeData : EdgeData<ActionSwitcher>
    {
        public override string name => "Next Action";
        public override string description => "The next action.";

        public ActionEdgeData()
        {
        }
        public ActionEdgeData(ActionSwitcher element) : base(element)
        {
        }
    }
}
