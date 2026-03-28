using NovaLine.Editor.Utils;
using NovaLine.Element.Event;
using NovaLine.Element.Switcher;
using UnityEngine;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Data.NodeGraphView;
    using NovaLine.Editor.File;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Editor.Window.Command;
    using NovaLine.Editor.Window;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Element;
    using System.Linq;
    using static NovaLine.Editor.Window.WindowContextRegistry;

    public class ConditionGraphView : NovaGraphView<EventGraphNode,Condition,NovaEvent,EventSwitcher>
    {
        protected override Color themedColor => ColorExt.EVENT_THEMED_COLOR;
        public ConditionGraphView(Condition linkedCondition) : base(linkedCondition) { }
        protected override void setEdgeUnpassable(GraphEdge<NovaEvent, EventSwitcher> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.2f;
        }
        public override EventGraphNode summonNewGraphNode(Vector2 pos)
        {
            var actualName = graphElements.Count().ToString();
            var newActionGraphNode = new EventGraphNode(new NovaEvent(actualName), pos);
            return newActionGraphNode;
        }
        public override EventGraphNode summonNewGraphNode(NovaEvent novaEvent, Vector2 pos)
        {
            return new EventGraphNode(novaEvent, pos);
        }
        public override IGraphViewContext summonNewChildGraphContext(NovaElement novaEvent, Vector2 pos)
        {
            return new EventContext(new EventData((NovaEvent)novaEvent, pos));
        }
        public override IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonAndConnectEdge<EventGraphEdge>((EventSwitcher)switcher);
        }
        protected override void updateNodes()
        {
            if(linkedElement?.conditionType != ConditionType.Sort)
            {
                foreach (var eventGraphNode in graphNodes)
                {
                    if (eventGraphNode == null) continue;
                    setNodePassable(eventGraphNode);
                }
            }
            else base.updateNodes();
        }
        protected override void updateEdges()
        {
            if(linkedElement?.conditionType != ConditionType.Sort)
            {
                foreach(var graphEdge in graphEdges)
                {
                    if (graphEdge == null || graphEdge is not EventGraphEdge eventEdge) continue;
                    setEdgeUnpassable(eventEdge);
                }
            }
            else base.updateEdges();
        }
    }
}