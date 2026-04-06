using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Element;
using System.Linq;

namespace NovaLine.Script.Editor.Graph.View
{
    public class ConditionGraphView : NovaGraphView<EventGraphNode,Condition,NovaEvent,EventSwitcher>
    {
        protected override Color themedColor => ColorExt.EVENT_THEMED_COLOR;
        public ConditionGraphView(string linkedConditionGuid) : base(linkedConditionGuid) { }
        protected override void setEdgeUnpassable(GraphEdge<NovaEvent, EventSwitcher> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.2f;
        }
        public override EventGraphNode summonNewGraphNode(Vector2 pos)
        {
            var actualName = linkedElement.childrenGuidList.Count.ToString();
            var newEvent = new NovaEvent(actualName);
            var newActionGraphNode = new EventGraphNode(newEvent, pos);
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
        public override IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return summonAndConnectEdge<EventGraphEdge>((EventSwitcher)linkedSwitcher);
        }
        protected override void updateNodes()
        {
            if (linkedElement?.conditionType != ConditionType.Sort)
            {
                foreach (var eventGraphNode in graphNodes.Values)
                {
                    if (eventGraphNode == null) continue;
                    setNodePassable(eventGraphNode);
                    eventGraphNode.update();
                }
            }
            else base.updateNodes();
        }
        protected override void updateEdges()
        {
            if(linkedElement?.conditionType != ConditionType.Sort)
            {
                foreach(var graphEdge in graphEdges.Values)
                {
                    if (graphEdge == null || graphEdge is not EventGraphEdge eventEdge) continue;
                    setEdgeUnpassable(eventEdge);
                }
            }
            else base.updateEdges();
        }
    }
}