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
        
        public override void SaveNodeData(List<GraphNode> graphNodes = null)
        {
            SaveNodeData<EventGraphNode, EventContext>(graphNodes == null ? null : graphNodes.Cast<EventGraphNode>().ToList());
        }

        public override void SaveEdgeData(List<IGraphEdge> graphEdges = null)
        {
            SaveEdgeData<EventEdgeData>(graphEdges);
        }

        protected override ConditionGraphView SummonGraphView()
        {
            return new ConditionGraphView(LinkedData.GUID);
        }
    }
}
