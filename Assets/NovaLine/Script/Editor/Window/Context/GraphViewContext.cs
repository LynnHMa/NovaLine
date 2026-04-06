﻿﻿using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Editor.Graph.Node;
using NovaLine.Script.Editor.Graph.View;
using NovaLine.Script.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using NovaLine.Script.Data.Edge;
using NovaLine.Script.Data.NodeGraphView;
using static NovaLine.Script.Editor.Window.Context.ContextInfo;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Element;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Editor.Window.Context
{
    /// <summary>
    /// Just a simple bridge from data to graph view.
    /// </summary>
    /// <typeparam name="GV">GraphView</typeparam>
    /// <typeparam name="GVND">Linked GraphViewNodeData</typeparam>
    [ContextInfo(AsNode.False, AsGraphView.False)]
    public abstract class GraphViewContext<GV, GVND> : IGraphViewContext where GV : INovaGraphView where GVND : IGraphViewNodeData
    {
        protected NovaWindow window => NovaWindow.Instance;
        private GV _graphView;
        public CommandRegistry commandRegistry;
        protected GraphViewContext(GVND linkedData)
        {
            this.linkedData = linkedData;
        }
        
        public virtual GV graphView
        {
            get
            {
                _graphView ??= summonGraphView();
                return _graphView;
            }
            set => _graphView = value;
        }
        public virtual GVND linkedData { get; set; }

        public virtual bool hasDrawn { get; set; } = false;
        public virtual NovaElementType type => linkedData != null && linkedData.linkedElement != null ? linkedData.linkedElement.type : NovaElementType.NONE;
        public virtual string title => graphView?.getActualName();
        public virtual string guid => linkedData?.guid;

        INovaGraphView IGraphViewContext.graphView { get => graphView; set => graphView = (GV)value; }
        IGraphViewNodeData IGraphViewContext.linkedData { get => linkedData; set => linkedData = (GVND)value; }
        NovaElementType IGraphViewContext.type { get => type; set { } }
        CommandRegistry IGraphViewContext.commandRegistry { get => commandRegistry; set => commandRegistry = value; }
        string IGUID.guid { get => guid; set { } }
        
        public virtual void saveData()
        {
            if (linkedData == null || graphView == null || window == null || !hasDrawn) return;
            saveNodeData();
            saveEdgeData();
        }
        protected virtual void saveNodeData<N, C>(List<N> graphNodes = null)
            where N : GraphNode
            where C : IGraphViewContext
        {
            var newNodeDatas = new EList<IGraphViewNodeData>();
            
            //Must redraw
            if (graphNodes != null && graphView != null)
            {
                disposeGraphView();
            }
            
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

                    newNodeDatas.Add(childContext.linkedData);
                }
            }

            linkedData.nodeDataList = newNodeDatas;
        }
        protected virtual void saveEdgeData<ED>(List<IGraphEdge> graphEdges = null) where ED : IEdgeData, new()
        {
            var newEdgeDatas = new EList<IEdgeData>();
            
            //Must redraw
            if (graphEdges != null)
            {
                disposeGraphView();
            }
            
            var actualGraphEdges = graphEdges ?? graphView.graphEdges?.Cast<IGraphEdge>().ToList();
            if (actualGraphEdges != null)
            {
                foreach (var checkingGraphEdge in actualGraphEdges)
                {
                    var newEdgeData = new ED();
                    newEdgeData.init(checkingGraphEdge.linkedElement);

                    newEdgeDatas.Add(newEdgeData);
                }
            }

            linkedData.edgeDataList = newEdgeDatas;
        }
        public abstract void saveNodeData(List<GraphNode> graphNodes = null);
        public abstract void saveEdgeData(List<IGraphEdge> graphEdges = null);
        
        public virtual void draw()
        {
            try
            {
                if (window == null || hasDrawn) return;

                commandRegistry = new();

                try
                {
                    using(new SaveScope())
                    using(new UpdateScope())
                    {
                        drawNode();
                        drawEdge();
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
                
                hasDrawn = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        public virtual void drawNode()
        {
            var nodeDataList = linkedData.nodeDataList;
            linkedData.linkedElement.childrenGuidList.Clear();
            if (nodeDataList == null || nodeDataList.Count == 0) return;
            for (int i = nodeDataList.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDataList[i];
                var graphNode = graphView.summonNewGraphNode(nodeData.linkedElement, nodeData.pos);
                if (graphNode != null)
                {
                    graphView.addGraphNode(graphNode, false);
                    if (graphNode.guid.Equals(linkedData.startGraphNodeGuid)) graphView.setFirstNode(graphNode,false);
                }
            }
        }
        public virtual void drawEdge()
        {
            var nodeEdgeDatas = linkedData.edgeDataList;
            if (nodeEdgeDatas == null || nodeEdgeDatas.Count == 0) return;
            foreach (var nodeEdgeData in nodeEdgeDatas)
            {
                var nodeGraphEdge = graphView.summonNewGraphEdge(nodeEdgeData.linkedSwitcher);
                if (nodeGraphEdge != null) graphView.addGraphEdge(nodeGraphEdge, false);
            }
        }
        protected abstract GV summonGraphView();

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
    public interface IGraphViewContext : IGUID
    {
        public string title { get; }
        public INovaGraphView graphView { get; set; }
        public IGraphViewNodeData linkedData { get; set; }
        public NovaElementType type { get; set; }
        public CommandRegistry commandRegistry { get; set; }
        public void saveData();
        public void saveNodeData(List<GraphNode> graphNodes);
        public void saveEdgeData(List<IGraphEdge> graphEdges);
        public void draw();
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
