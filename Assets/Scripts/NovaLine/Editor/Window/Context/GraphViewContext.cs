using NovaLine.Editor.Graph.Edge;
using NovaLine.Editor.Graph.Node;
using NovaLine.Editor.Graph.View;
using NovaLine.Utils.Interface;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using NovaLine.Data.Edge;
using NovaLine.Data.NodeGraphView;
using static NovaLine.Editor.Window.Context.ContextInfo;
using static NovaLine.Editor.Window.WindowContextRegistry;
using NovaLine.Editor.Utils.Ext;
using NovaLine.Element;

namespace NovaLine.Editor.Window.Context
{
    [ContextInfo(AsNode.False, AsGraphView.False)]
    public abstract class GraphViewContext<GV, GVND> : IGraphViewContext where GV : INovaGraphView where GVND : IGraphViewNodeData
    {
        protected NovaWindow window => NovaWindow.Instance;
        public CommandRegistry commandRegistry = new();
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
        public virtual NovaElementType type => linkedData != null && linkedData.linkedElement != null ? linkedData.linkedElement.type : NovaElementType.NONE;
        public virtual GVND linkedData { get; set; }
        public virtual string title => graphView?.getActualName();
        public virtual string guid => linkedData?.guid;

        INovaGraphView IGraphViewContext.graphView { get => graphView; set => graphView = (GV)value; }
        IGraphViewNodeData IGraphViewContext.linkedData { get => linkedData; set => linkedData = (GVND)value; }
        NovaElementType IGraphViewContext.type { get => type; set { } }
        CommandRegistry IGraphViewContext.commandRegistry { get => commandRegistry; set => commandRegistry = value; }
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

                    var context = GetContext(checkingGraphNode, type + 1);
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
            cleanInvalidChild();
            
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
                    graphView.addGraphNode(graphNode, true, false, false);
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
                if (nodeGraphEdge != null) graphView.addGraphEdge(nodeGraphEdge, true, false,false);
            }
        }

        protected abstract void cleanInvalidChild();

        protected abstract GV summonGraphView();
    }
    public interface IGraphViewContext : IGUID
    {
        public string title { get; }
        public INovaGraphView graphView { get; set; }
        public IGraphViewNodeData linkedData { get; set; }
        public NovaElementType type { get; set; }
        public CommandRegistry commandRegistry { get; set; }
        public void save();
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
