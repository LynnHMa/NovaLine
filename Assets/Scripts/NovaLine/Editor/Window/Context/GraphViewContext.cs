using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using static NovaLine.Editor.Window.Context.ContextInfo;
using NovaLine.Editor.Graph.Data.NodeGraphView;
using NovaLine.Editor.Graph.Data.Edge;
using NovaLine.Utils.Interface;
using System;
using NovaLine.Utils.Ext;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.False)]
    public abstract class GraphViewContext<GV, GVND> : IGraphViewContext where GV : INovaGraphView where GVND : IGraphViewNodeData
    {
        protected NovaWindow window => NovaWindow.GetMainWindowInstance();
        public GraphViewContext(GVND linkedData)
        {
            this.linkedData = linkedData;
        }
        public GraphViewContext(GV graphView, GVND linkedData)
        {
            this.graphView = graphView;
            this.linkedData = linkedData;
        }
        public virtual bool hasDrawn { get; set; } = false;
        private GV _graphView;
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
        public virtual ContextType type { get; set; } = ContextType.NONE;
        public virtual GVND linkedData { get; set; }
        public virtual string title => graphView?.getActualName();
        public virtual string guid => linkedData?.guid;

        INovaGraphView IGraphViewContext.graphView { get => graphView; set => graphView = (GV)value; }
        IGraphViewNodeData IGraphViewContext.linkedData { get => linkedData; set => linkedData = (GVND)value; }
        string IGUID.guid { get => guid; set { } }

        public abstract void save();
        protected virtual void save<N, C, ED>()
            where N : GraphNode
            where C : IGraphViewContext
            where ED : IEdgeData, new()
        {
            if (linkedData == null || graphView == null || window == null || !hasDrawn) return;

            saveNode<N, C>();
            saveEdge<ED>();
        }
        protected virtual void saveNode<N, C>()
            where N : GraphNode
            where C : IGraphViewContext
        {
            var newNodeDatas = new EList<IGraphViewNodeData>();

            var actualGraphNodes = graphView.graphNodes.Cast<N>().ToList();
            if (actualGraphNodes != null)
            {
                foreach (var checkingGraphNode in actualGraphNodes)
                {
                    if (checkingGraphNode == null) continue;

                    var context = NovaWindow.GetContext(checkingGraphNode, type + 1);
                    if (context == null || context is not C childContext) continue;

                    childContext.save();
                    childContext.linkedData.pos = checkingGraphNode.pos;

                    newNodeDatas.Add(childContext.linkedData);
                }
            }

            linkedData.nodeDatas = newNodeDatas;
        }
        protected virtual void saveEdge<ED>() where ED : IEdgeData, new()
        {
            var newEdgeDatas = new EList<IEdgeData>();
            var actualGraphEdges = graphView.graphEdges;
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

        public virtual void draw()
        {
            if (window == null || hasDrawn) return;

            drawNode();
            drawEdge();

            if (graphView is GraphView gv)
            {
                window.rootVisualElement?.Add(gv);
            }

            hasDrawn = true;
        }

        public virtual void drawNode()
        {
            var nodeDatas = linkedData.nodeDatas;
            if (nodeDatas == null || nodeDatas.Count == 0) return;
            for (int i = nodeDatas.Count - 1; i >= 0; i--)
            {
                var nodeData = nodeDatas[i];
                var graphNode = graphView.summonNewGraphNode(nodeData.linkedElement, nodeData.pos);
                if (graphNode != null)
                {
                    graphView.addGraphNode(graphNode, true, false);
                    if (graphNode.guid.Equals(linkedData.startGraphNodeGuid)) graphView.firstNode = graphNode;
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
                if (nodeGraphEdge != null) graphView.addGraphEdge(nodeGraphEdge, true, false);
            }
        }

        protected abstract GV summonGraphView();
    }
    public interface IGraphViewContext : IGUID
    {
        public string title { get; }
        public INovaGraphView graphView { get; set; }
        public IGraphViewNodeData linkedData { get; set; }
        public ContextType type { get; set; }
        public void save();
        public void draw();
    }
    public enum ContextType
    {
        NONE,
        FLOWCHART,
        NODE,
        ACTION,
        CONDITION,
        EVENT
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
