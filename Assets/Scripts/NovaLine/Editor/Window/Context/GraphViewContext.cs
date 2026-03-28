using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using static NovaLine.Editor.Window.Context.ContextInfo;
using static NovaLine.Editor.Window.WindowContextRegistry;
using NovaLine.Editor.Utils.Ext;
using NovaLine.Editor.Utils.Scope;
using NovaLine.Element;
using UnityEditor;

namespace NovaLine.Editor.Window.Context
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
        private GVND _linkedData;
        public CommandRegistry commandRegistry;
        protected GraphViewContext(GVND linkedData)
        {
            this.linkedData = linkedData;
        }
        
        public virtual GV graphView
        {
            get
            {
                if (_graphView == null) _graphView = summonGraphView();
                return _graphView;
            }
            set
            {
                _graphView = value;
            }
        }
        public virtual GVND linkedData
        {
            get => _linkedData;
            set => _linkedData = value;
        }
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
                    if (context == null || context is not C childContext) continue;

                    childContext.saveData();
                    childContext.linkedData.pos = checkingGraphNode.pos;

                    newNodeDatas.Add(childContext.linkedData);
                }
            }

            linkedData.nodeDatas = newNodeDatas;
        }
        protected virtual void saveEdgeData<ED>(List<IGraphEdge> graphEdges = null) where ED : IEdgeData, new()
        {
            var newEdgeDatas = new EList<IEdgeData>();
            
            //Must redraw
            if (graphEdges != null)
            {
                disposeGraphView();
            }
            
            var actualGraphEdges = graphEdges ?? graphView.graphEdges;
            if (actualGraphEdges != null)
            {
                foreach (var item in actualGraphEdges)
                {
                    if (item is not IGraphEdge checkingGraphEdge) continue;

                    var newEdgeData = new ED();
                    newEdgeData.onSummon(checkingGraphEdge.linkedElement);

                    newEdgeDatas.Add(newEdgeData);
                }
            }

            linkedData.edgeDatas = newEdgeDatas;
        }
        public abstract void saveNodeData(List<GraphNode> graphNodes = null);
        public abstract void saveEdgeData(List<IGraphEdge> graphEdges = null);
        
        public virtual void draw()
        {
            if (window == null || hasDrawn) return;

            commandRegistry = new();
            
            using (new SaveScope())
            using(new UpdateScope())
            {
                drawNode();
                EditorApplication.delayCall += drawEdge;
            }
            
            if (graphView is GraphView gv)
            {
                window.rootVisualElement?.Add(gv);
            }

            hasDrawn = true;
        }
        public virtual void drawNode()
        {
            var nodeDatas = linkedData.nodeDatas;
            linkedData.linkedElement.children.Clear();
            if (nodeDatas == null || nodeDatas.Count == 0) return;
            for (int i = nodeDatas.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDatas[i];
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
            var nodeEdgeDatas = linkedData.edgeDatas;
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
