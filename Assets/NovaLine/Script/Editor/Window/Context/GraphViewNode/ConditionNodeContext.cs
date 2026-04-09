﻿using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using static NovaLine.Script.Editor.Window.Context.GraphViewNode.ContextInfo;

namespace NovaLine.Script.Editor.Window.Context.GraphViewNode
{
    [ContextInfo(AsNode.False, AsGraphView.True)]
    public class ConditionContext : GraphViewNodeContext<ConditionGraphView, ConditionData>
    {
        public ConditionContext(ConditionData linkedData) : base(linkedData) { }

        public override void saveNodeData(List<GraphNode> graphNodes = null)
        {
            saveNodeData<EventGraphNode, EventNodeContext>(graphNodes == null ? null : graphNodes.Cast<EventGraphNode>().ToList());
        }

        public override void saveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            saveEdgeData<EventEdgeData>(graphEdges);
        }

        protected override ConditionGraphView summonGraphView()
        {
            return new ConditionGraphView(linkedData.guid);
        }
    }
}
