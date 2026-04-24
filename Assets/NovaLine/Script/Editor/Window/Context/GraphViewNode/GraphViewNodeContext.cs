using System;
using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Context.Edge;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
            if (LinkedData == null) return;
            
            if (HasDrawn && _graphView != null && Window != null)
            {
                SaveNodeData();
                SaveEdgeData();
            }
            else
            {
                SaveChildDataDirectly();
            }
            
            SaveConditionData();
        }
        protected virtual void SaveNodeData<TGraphNode, TContext>(List<TGraphNode> graphNodes = null)
            where TGraphNode : GraphNode
            where TContext : IGraphViewNodeContext
        {
            var newNodeDataList = new ListExt<IGraphViewNodeData>();
            
            var actualGraphNodes = graphNodes ?? GraphView.GraphNodes.Cast<TGraphNode>().ToList();
            if (actualGraphNodes.Count != 0)
            {
                foreach (var checkingGraphNode in actualGraphNodes)
                {
                    if (checkingGraphNode == null) continue;

                    var context = GetContext(checkingGraphNode, checkingGraphNode.Type);
                    if (context is not TContext childContext) continue;

                    childContext.SaveData();
                    childContext.LinkedData.Pos = checkingGraphNode.Pos;

                    newNodeDataList.Add(childContext.LinkedData);
                }
            }

            LinkedData.NodeDataList = newNodeDataList;
            
            //Must redraw
            if (graphNodes != null && GraphView != null)
            {
                DisposeGraphView();
            }
        }
        protected virtual void SaveEdgeData<TEdgeData>(List<IGraphEdge> graphEdges = null) 
            where TEdgeData : IEdgeData, new()
        {
            var newEdgeDataList = new ListExt<IEdgeData>();
            
            var actualGraphEdges = graphEdges ?? GraphView.GraphEdges?.Cast<IGraphEdge>().ToList();
            if (actualGraphEdges != null)
            {
                foreach (var checkingGraphEdge in actualGraphEdges)
                {
                    var context = GetContext(checkingGraphEdge.GUID, NovaElementType.Switcher);
                    if (context is not EdgeContext edgeContext) continue;
                    context.SaveData();
                    
                    newEdgeDataList.Add(edgeContext.LinkedData);
                }
            }

            LinkedData.EdgeDataList = newEdgeDataList;
            
            //Must redraw
            if (graphEdges != null && GraphView != null)
            {
                DisposeGraphView();
            }
        }

        protected virtual void SaveChildDataDirectly()
        {
            if (LinkedData.NodeDataList != null)
            {
                foreach (var childNodeData in LinkedData.NodeDataList)
                {
                    var childContext = GetContext(childNodeData.GUID, childNodeData.LinkedElement.Type);
                    childContext?.SaveData();
                }
            }
                
            if (LinkedData.EdgeDataList != null)
            {
                foreach (var childEdgeData in LinkedData.EdgeDataList)
                {
                    var childContext = GetContext(childEdgeData.GUID, NovaElementType.Switcher);
                    childContext?.SaveData();
                }
            }
        }
        protected virtual void SaveConditionData()
        {
            ConditionData HandleConditionContext(string conditionGUID)
            {
                if (string.IsNullOrEmpty(conditionGUID)) return null;
                if (GetContext(conditionGUID, NovaElementType.Condition) is not ConditionContext conditionContext) return null;
                conditionContext.SaveData();
                conditionContext.LinkedData.Pos = Vector2.zero;
                return conditionContext.LinkedData;
            }
            
            if (LinkedData is not IAroundConditionData aroundConditionData) return;
            
            var beforeConditionData = aroundConditionData.ConditionBeforeInvokeData;
            var afterConditionData = aroundConditionData.ConditionAfterInvokeData;
            
            if(beforeConditionData == null || afterConditionData == null) return;
            
            aroundConditionData.ConditionBeforeInvokeData = HandleConditionContext(beforeConditionData.GUID);
            aroundConditionData.ConditionAfterInvokeData = HandleConditionContext(afterConditionData.GUID);
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
            var nodeDataList = LinkedData.NodeDataList;
            if (nodeDataList == null || nodeDataList.Count == 0) return;
            for (int i = nodeDataList.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDataList[i];
                var graphNode = GraphView.SummonNewGraphNode(nodeData.LinkedElement, nodeData.Pos);
                if (graphNode != null)
                {
                    GraphView.AddGraphNode(graphNode);
                }
            }
            if(!string.IsNullOrEmpty(LinkedData?.LinkedElement?.FirstChildGUID)) GraphView.SetFirstNode(LinkedData.LinkedElement.FirstChildGUID,false);
        }
        public virtual void DrawEdge()
        {
            var edgeDataList = LinkedData.EdgeDataList;
            if (edgeDataList == null || edgeDataList.Count == 0) return;
            foreach (var edgeData in edgeDataList)
            {
                var nodeGraphEdge = GraphView.SummonNewGraphEdge(edgeData.LinkedElement);
                if (nodeGraphEdge != null) GraphView.AddGraphEdge(nodeGraphEdge);
            }
        }
        
        protected abstract TGraphView SummonGraphView();

        public override void ReplaceLinkedData(INovaData linkedData)
        {
            base.ReplaceLinkedData(linkedData);
            
            if (GraphView != null && !CurrentGraphViewNodeContext.GUID.Equals(GUID))
            {
                DisposeGraphView();
            }
        }
        
        public void DisposeGraphView()
        {
            if (!HasDrawn && _graphView == null) return;
            if (_graphView is GraphView gv)
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
        new IGraphViewNodeData LinkedData { get; set; }
        CommandRegistry CommandRegistry { get; }
        void SaveNodeData(List<GraphNode> graphNodes);
        void SaveEdgeData(List<IGraphEdge> graphEdges);
        void Draw();
        void DisposeGraphView();
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
