using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class ActionEdgeData : EdgeData<ActionSwitcher>
    {
        public override string Name => "Next Action";
        public override string Description => "The next action.";

        public ActionEdgeData()
        {
        }
        public ActionEdgeData(ActionSwitcher element) : base(element)
        {
        }
    }
}
