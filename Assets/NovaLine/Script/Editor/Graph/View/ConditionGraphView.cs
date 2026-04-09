using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Element;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;

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
            var actualName = (linkedElement.childrenGuidList.Count + 1).ToString();
            var newEvent = new NovaEvent(actualName);
            var newActionGraphNode = new EventGraphNode(newEvent, pos);
            return newActionGraphNode;
        }
        public override EventGraphNode summonNewGraphNode(NovaEvent novaEvent, Vector2 pos)
        {
            return new EventGraphNode(novaEvent, pos);
        }
        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return summonNewChildGraphViewNodeContext(new EventData(linkedElement as NovaEvent,pos));
        }

        public override IGraphViewNodeContext summonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new EventNodeContext(linkedData as EventData);
        }
        
        public override EdgeContext summonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new EventEdgeData(linkedSwitcher as EventSwitcher));
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
                    if (graphEdge is not EventGraphEdge eventEdge) continue;
                    setEdgeUnpassable(eventEdge);
                }
            }
            else base.updateEdges();
        }
    }
}