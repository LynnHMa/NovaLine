using NovaLine.Editor.Utils;
using NovaLine.Element.Switcher;
using UnityEngine;
namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Action;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using System.Linq;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Data.NodeGraphView;

    public class NodeGraphView : NovaGraphView<ActionGraphNode,Node,NovaAction,ActionSwitcher>
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public NodeGraphView(Node linkedNode) : base(linkedNode) { }
        public override ActionGraphNode summonNewGraphNode(Vector2 pos)
        {
            var actualName = graphElements.Count().ToString();
            var newActionGraphNode = new ActionGraphNode(new NovaAction(actualName), pos);
            return newActionGraphNode;
        }
        public override ActionGraphNode summonNewGraphNode(NovaAction novaAction, Vector2 pos)
        {
            return new ActionGraphNode(novaAction, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(NovaElement action, Vector2 pos)
        {
            return new ActionContext(new ActionData((NovaAction)action, pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<ActionGraphEdge>((ActionSwitcher)switcher);
        }
        protected override void updateNodes()
        {
            base.updateNodes();

            foreach(var graphNode in graphNodes)
            {
                if(graphNode.linkedElement is NovaAction action && action.actionType == ActionType.Meanwhile)
                {
                    setNodePassable(graphNode);
                }
            }
        }
    }
}