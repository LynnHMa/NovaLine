using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Element;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Utils.Ext;

namespace NovaLine.Script.Editor.Graph.View
{
    public class ConditionGraphView : NovaGraphView<EventGraphNode,Condition,NovaEvent,EventSwitcher>
    {
        protected override Color ThemedColor => ColorExt.EVENT_THEMED_COLOR;
        public ConditionGraphView(string linkedConditionGUID) : base(linkedConditionGUID) { }
        protected override void SetEdgeUnpassable(GraphEdge<NovaEvent, EventSwitcher> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.2f;
        }
        public override EventGraphNode SummonNewGraphNode(Vector2 pos)
        {
            var actualName = (LinkedElement.ChildrenGUIDList.Count + 1).ToString();
            var newEvent = new NovaEvent(actualName);
            var newActionGraphNode = new EventGraphNode(newEvent, pos);
            return newActionGraphNode;
        }
        public override EventGraphNode SummonNewGraphNode(NovaEvent novaEvent, Vector2 pos)
        {
            return new EventGraphNode(novaEvent, pos);
        }
        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(NovaElement linkedElement, Vector2 pos)
        {
            return SummonNewChildGraphViewNodeContext(new EventData(linkedElement as NovaEvent,pos));
        }

        public override IGraphViewNodeContext SummonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return new EventContext(linkedData as EventData);
        }
        
        public override EdgeContext SummonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return new EdgeContext(new EventEdgeData(linkedSwitcher as EventSwitcher));
        }
        
        public override IGraphEdge SummonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return SummonAndConnectEdge<EventGraphEdge>((EventSwitcher)linkedSwitcher);
        }
        protected override void UpdateNodes()
        {
            if (LinkedElement?.ConditionType != ConditionType.Sort)
            {
                foreach (var eventGraphNode in GraphNodes.Values)
                {
                    if (eventGraphNode == null) continue;
                    SetNodePassable(eventGraphNode);
                    eventGraphNode.Update();
                }
            }
            else base.UpdateNodes();
        }
        protected override void UpdateEdges()
        {
            if(LinkedElement?.ConditionType != ConditionType.Sort)
            {
                foreach(var graphEdge in GraphEdges.Values)
                {
                    if (graphEdge is not EventGraphEdge eventEdge) continue;
                    SetEdgeUnpassable(eventEdge);
                }
            }
            else base.UpdateEdges();
        }
    }
}