using UnityEngine;
using System;

namespace NovaLine.Editor.Graph.View
{
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Switcher;
    using System.Linq;
    using NovaLine.Editor.File;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Event;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Editor.Graph.Data.NodeGraphView;
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
                root.firstEvent = (NovaEvent)value.linkedElement;
            }
        }
        public ConditionGraphView(Condition root) : base(root,root.name) { }
        protected override string getType()
        {
            return "[Condition]";
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
        public override void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphEdge(graphEdge, isLoading, autoSave);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true)
        {
            base.removeGraphEdge(graphEdge, autoSave);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true)
        {
            base.addGraphNode(graphNode, isLoading, autoSave);

            if (!isLoading)
            {
                root.novaEvents.Add((NovaEvent)graphNode.linkedElement);
                
            }

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
        public override void removeGraphNode(GraphNode graphNode, bool autoSave = true)
        {
            base.removeGraphNode(graphNode, autoSave);

            root.novaEvents.Remove((NovaEvent)graphNode.linkedElement);

            if (autoSave)
            {
                NovaFileManager.SaveGraphWindowData();
            }
        }
    }
}