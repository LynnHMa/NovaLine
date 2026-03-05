using NovaLine.Action;
using NovaLine.Editor.Graph.Node;
using NovaLine.Switcher;
using System;

namespace NovaLine.Editor.Graph.Data
{
    [Serializable]
    public class ActionEdgeGraphViewData : GraphViewEdgeData<NovaAction,ActionSwitcher,NodeGraphViewData,Element.Node, ActionGraphNode>
    {
        public override string name => "Next Action";

        public override string describtion => "The next action.";
        public ActionEdgeGraphViewData(ActionSwitcher switcher) : base(switcher)
        {
        }
    }
}
