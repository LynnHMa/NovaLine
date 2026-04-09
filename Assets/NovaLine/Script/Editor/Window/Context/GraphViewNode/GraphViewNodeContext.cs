using System;
using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Element;
using UnityEditor.Experimental.GraphView;
using static NovaLine.Script.Editor.Window.Context.GraphViewNode.ContextInfo;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Window.Context.GraphViewNode
{
    /// <summary>
    /// Just a simple bridge from graph view node data to graph view.
    /// </summary>
    /// <typeparam name="TGraphView">GraphView</typeparam>
    /// <typeparam name="TLinkedGraphViewNodeData">Linked GraphViewNodeData</typeparam>
    [ContextInfo(AsNode.False, AsGraphView.False)]
    public abstract class GraphViewNodeContext<TGraphView, TLinkedGraphViewNodeData> : NovaContext<TLinkedGraphViewNodeData>,IGraphViewNodeContext where TGraphView : INovaGraphView where TLinkedGraphViewNodeData : IGraphViewNodeData
    {
        private TGraphView _graphView;
        public CommandRegistry commandRegistry;
        protected GraphViewNodeContext(TLinkedGraphViewNodeData linkedData) : base(linkedData){}
        
        public TGraphView graphView
        {
            get
            {
                _graphView ??= summonGraphView();
                return _graphView;
            }
            set => _graphView = value;
        }
        public bool hasDrawn { get; set; }
        public string title => graphView?.getActualName();
        
        INovaGraphView IGraphViewNodeContext.graphView { get => graphView; set => graphView = (TGraphView)value; }
        IGraphViewNodeData IGraphViewNodeContext.linkedData { get => linkedData; set => linkedData = (TLinkedGraphViewNodeData)value; }
        CommandRegistry IGraphViewNodeContext.commandRegistry => commandRegistry;
        
        public override void saveData()
        {
            if (linkedData == null || graphView == null || window == null || !hasDrawn) return;
            saveNodeData();
            saveEdgeData();
        }
        protected virtual void saveNodeData<N, C>(List<N> graphNodes = null)
            where N : GraphNode
            where C : IGraphViewNodeContext
        {
            var newNodeDataList = new EList<IGraphViewNodeData>();
            
            var actualGraphNodes = graphNodes ?? graphView.graphNodes.Cast<N>().ToList();
            if (actualGraphNodes.Count != 0)
            {
                foreach (var checkingGraphNode in actualGraphNodes)
                {
                    if (checkingGraphNode == null) continue;

                    var context = GetContext(checkingGraphNode, checkingGraphNode.type);
                    if (context is not C childContext) continue;

                    childContext.saveData();
                    childContext.linkedData.pos = checkingGraphNode.pos;

                    newNodeDataList.Add(childContext.linkedData);
                }
            }

            linkedData.nodeDataList = newNodeDataList;
            
            //Must redraw
            if (graphNodes != null && graphView != null)
            {
                disposeGraphView();
            }
        }
        protected virtual void saveEdgeData<ED>(List<IGraphEdge> graphEdges = null) where ED : IEdgeData, new()
        {
            var newEdgeDataList = new EList<IEdgeData>();
            
            var actualGraphEdges = graphEdges ?? graphView.graphEdges?.Cast<IGraphEdge>().ToList();
            if (actualGraphEdges != null)
            {
                foreach (var checkingGraphEdge in actualGraphEdges)
                {
                    var context = GetContext(checkingGraphEdge.guid, NovaElementType.SWITCHER);
                    if (context is not EdgeContext edgeContext) continue;
                    context.saveData();
                    
                    newEdgeDataList.Add(edgeContext.linkedData);
                }
            }

            linkedData.edgeDataList = newEdgeDataList;
            
            //Must redraw
            if (graphEdges != null && graphView != null)
            {
                disposeGraphView();
            }
        }
        public abstract void saveNodeData(List<GraphNode> graphNodes = null);
        public abstract void saveEdgeData(List<IGraphEdge> graphEdges = null);
        
        public virtual void draw()
        {
            if (window == null || hasDrawn) return;

            commandRegistry = new();

            using(new SaveScope(true))
            using(new UpdateScope())
            {
                drawNode();
                drawEdge();
            }
                
            hasDrawn = true;
        }
        public virtual void drawNode()
        {
            var nodeDataList = linkedData.nodeDataList;
            if (nodeDataList == null || nodeDataList.Count == 0) return;
            for (int i = nodeDataList.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDataList[i];
                var graphNode = graphView.summonNewGraphNode(nodeData.linkedElement, nodeData.pos);
                if (graphNode != null)
                {
                    graphView.addGraphNode(graphNode);
                }
            }
            if(!String.IsNullOrEmpty(linkedData.linkedElement.firstChildGuid)) graphView.setFirstNode(linkedData.linkedElement.firstChildGuid,false);
        }
        public virtual void drawEdge()
        {
            var nodeEdgeDatas = linkedData.edgeDataList;
            if (nodeEdgeDatas == null || nodeEdgeDatas.Count == 0) return;
            foreach (var nodeEdgeData in nodeEdgeDatas)
            {
                var nodeGraphEdge = graphView.summonNewGraphEdge(nodeEdgeData.linkedElement);
                if (nodeGraphEdge != null) graphView.addGraphEdge(nodeGraphEdge);
            }
        }
        
        protected abstract TGraphView summonGraphView();

        public void disposeGraphView()
        {
            if (!hasDrawn && graphView == null) return;
            if (graphView is GraphView gv)
            {
                gv.Clear();
                gv.RemoveFromHierarchy();
            } 
            graphView = default;
            hasDrawn = false;
        }
        
    }
    public interface IGraphViewNodeContext : INovaContext
    {
        string title { get; }
        INovaGraphView graphView { get; set; }
        new IGraphViewNodeData linkedData { get; set; }
        CommandRegistry commandRegistry { get; }
        void saveNodeData(List<GraphNode> graphNodes);
        void saveEdgeData(List<IGraphEdge> graphEdges);
        void draw();
    }
    public class ContextInfo : Attribute
    {
        public AsNode asNode { get; set; }
        public AsGraphView asGraphView { get; set; }
        public ContextInfo(AsNode asNode, AsGraphView asGraphView)
        {
            this.asNode = asNode;
            this.asGraphView = asGraphView;
        }
        public enum AsNode
        {
            True,
            False
        }
        public enum AsGraphView
        {
            True,
            False
        }
    }
}
