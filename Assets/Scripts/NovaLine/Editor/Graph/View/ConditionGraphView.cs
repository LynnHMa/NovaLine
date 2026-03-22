using NovaLine.Element.Event;
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
    using NovaLine.Switcher;
    using System.Linq;
    using static NovaLine.Editor.Window.WindowContextRegistry;

    public class ConditionGraphView : NovaGraphView<EventGraphNode,Condition,NovaEvent,EventSwitcher>
    {
        public override EventGraphNode firstNode
        {
            get
            {
                return base.firstNode;
            }
            set
            {
                base.firstNode = value;
                linkedElement.firstEvent = (NovaEvent)value?.linkedElement;
            }
        }
        public ConditionGraphView(Condition linkedCondition) : base(linkedCondition,linkedCondition?.name) { }
        protected override string getType()
        {
            return "[Condition]";
        }
        protected override void setNodePassable(EventGraphNode graphNode)
        {
            if (graphNode == null || graphNode.isPassable || graphNode.linkedElement is not NovaEvent novaEvent) return;

            base.setNodePassable(graphNode);

            if (novaEvent.nextEvent == null || linkedElement.conditionType != ConditionType.Sort) return;

            var eventSwitcher = novaEvent.nextEvent;

            if (eventSwitcher.inputElement == null || eventSwitcher.outputElement == null) return;

            setNodePassable(getExistingGraphNode(eventSwitcher.inputElement.guid));
        }
        protected override void setNodeUnpassable(EventGraphNode graphNode)
        {
            if (graphNode == null || !graphNode.isPassable || graphNode.linkedElement is not NovaEvent novaEvent || linkedElement.conditionType != ConditionType.Sort) return;

            base.setNodeUnpassable(graphNode);

            if (novaEvent.nextEvent == null) return;

            var eventSwitcher = novaEvent.nextEvent;

            if (eventSwitcher.inputElement == null || eventSwitcher.outputElement == null) return;

            setNodeUnpassable(getExistingGraphNode(eventSwitcher.inputElement.guid));
        }
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
        public override IGraphViewContext summonNewChildGraphContext(NovaEvent novaEvent, Vector2 pos)
        {
            return new EventContext(new EventData(novaEvent, pos));
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
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true, bool registerCommand = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave, registerCommand);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphEdge(graphEdge, autoSave, registerCommand);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true, bool registerCommand = true)
        {
            base.addGraphNode(graphNode, isLoading, autoSave, registerCommand);

            if (!isLoading)
            {
                linkedElement.novaEvents.Add((NovaEvent)graphNode.linkedElement);
            }

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true, bool registerCommand = true)
        {
            base.removeGraphNode(graphNode, autoSave, registerCommand);

            linkedElement.novaEvents.Remove((NovaEvent)graphNode.linkedElement);
            UnregisterContext(graphNode.guid,NovaElementType.EVENT);

            if (autoSave)
            {
                EditorFileManager.SaveGraphWindowData();
            }
        }
    }
}