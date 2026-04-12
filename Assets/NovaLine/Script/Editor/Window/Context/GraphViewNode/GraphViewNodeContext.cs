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
        
        public TGraphView GraphView
        {
            get
            {
                _graphView ??= SummonGraphView();
                return _graphView;
            }
            set => _graphView = value;
        }
        public bool HasDrawn { get; set; }
        public string Title => GraphView?.GetActualName();
        
        INovaGraphView IGraphViewNodeContext.GraphView { get => GraphView; set => GraphView = (TGraphView)value; }
        IGraphViewNodeData IGraphViewNodeContext.LinkedData { get => LinkedData; set => LinkedData = (TLinkedGraphViewNodeData)value; }
        CommandRegistry IGraphViewNodeContext.CommandRegistry => commandRegistry;
        
        public override void SaveData()
        {
            if (LinkedData == null || GraphView == null || Window == null || !HasDrawn) return;
            SaveNodeData();
            SaveEdgeData();
        }
        protected virtual void SaveNodeData<N, C>(List<N> graphNodes = null)
            where N : GraphNode
            where C : IGraphViewNodeContext
        {
            var newNodeDataList = new EList<IGraphViewNodeData>();
            
            var actualGraphNodes = graphNodes ?? GraphView.GraphNodes.Cast<N>().ToList();
            if (actualGraphNodes.Count != 0)
            {
                foreach (var checkingGraphNode in actualGraphNodes)
                {
                    if (checkingGraphNode == null) continue;

                    var context = GetContext(checkingGraphNode, checkingGraphNode.type);
                    if (context is not C childContext) continue;

                    childContext.SaveData();
                    childContext.LinkedData.pos = checkingGraphNode.pos;

                    newNodeDataList.Add(childContext.LinkedData);
                }
            }

            LinkedData.nodeDataList = newNodeDataList;
            
            //Must redraw
            if (graphNodes != null && GraphView != null)
            {
                DisposeGraphView();
            }
        }
        protected virtual void SaveEdgeData<ED>(List<IGraphEdge> graphEdges = null) where ED : IEdgeData, new()
        {
            var newEdgeDataList = new EList<IEdgeData>();
            
            var actualGraphEdges = graphEdges ?? GraphView.GraphEdges?.Cast<IGraphEdge>().ToList();
            if (actualGraphEdges != null)
            {
                foreach (var checkingGraphEdge in actualGraphEdges)
                {
                    var context = GetContext(checkingGraphEdge.Guid, NovaElementType.SWITCHER);
                    if (context is not EdgeContext edgeContext) continue;
                    context.SaveData();
                    
                    newEdgeDataList.Add(edgeContext.LinkedData);
                }
            }

            LinkedData.edgeDataList = newEdgeDataList;
            
            //Must redraw
            if (graphEdges != null && GraphView != null)
            {
                DisposeGraphView();
            }
        }
        public abstract void SaveNodeData(List<GraphNode> graphNodes = null);
        public abstract void SaveEdgeData(List<IGraphEdge> graphEdges = null);
        
        public virtual void Draw()
        {
            if (Window == null || HasDrawn) return;

            commandRegistry = new();

            using(new SaveScope(true))
            using(new UpdateScope())
            {
                DrawNode();
                DrawEdge();
            }
                
            HasDrawn = true;
        }
        public virtual void DrawNode()
        {
            var nodeDataList = LinkedData.nodeDataList;
            if (nodeDataList == null || nodeDataList.Count == 0) return;
            for (int i = nodeDataList.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDataList[i];
                var graphNode = GraphView.SummonNewGraphNode(nodeData.linkedElement, nodeData.pos);
                if (graphNode != null)
                {
                    GraphView.AddGraphNode(graphNode);
                }
            }
            if(!String.IsNullOrEmpty(LinkedData.linkedElement.FirstChildGuid)) GraphView.SetFirstNode(LinkedData.linkedElement.FirstChildGuid,false);
        }
        public virtual void DrawEdge()
        {
            var nodeEdgeDatas = LinkedData.edgeDataList;
            if (nodeEdgeDatas == null || nodeEdgeDatas.Count == 0) return;
            foreach (var nodeEdgeData in nodeEdgeDatas)
            {
                var nodeGraphEdge = GraphView.SummonNewGraphEdge(nodeEdgeData.linkedElement);
                if (nodeGraphEdge != null) GraphView.AddGraphEdge(nodeGraphEdge);
            }
        }
        
        protected abstract TGraphView SummonGraphView();

        public void DisposeGraphView()
        {
            if (!HasDrawn && GraphView == null) return;
            if (GraphView is GraphView gv)
            {
                gv.Clear();
                gv.RemoveFromHierarchy();
            } 
            GraphView = default;
            HasDrawn = false;
        }
        
    }
    public interface IGraphViewNodeContext : INovaContext
    {
        string Title { get; }
        INovaGraphView GraphView { get; set; }
        IGraphViewNodeData LinkedData { get; set; }
        CommandRegistry CommandRegistry { get; }
        void SaveNodeData(List<GraphNode> graphNodes);
        void SaveEdgeData(List<IGraphEdge> graphEdges);
        void Draw();
    }
    public class ContextInfo : Attribute
    {
        public AsNode IsAsNode { get; set; }
        public AsGraphView IsAsGraphView { get; set; }
        public ContextInfo(AsNode isAsNode, AsGraphView isAsGraphView)
        {
            IsAsNode = isAsNode;
            IsAsGraphView = isAsGraphView;
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
