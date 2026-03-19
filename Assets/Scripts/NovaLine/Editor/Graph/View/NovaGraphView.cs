using System.Collections.Generic;
using System;
using NovaLine.Editor.Graph.Node;
using UnityEngine;

namespace NovaLine.Editor.Graph.View
{
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Element;
    using NovaLine.Editor.File;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Switcher;
    using NovaLine.Editor.Graph.Port;
    using System.Collections;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Utils.Ext;

    public abstract class NovaGraphView<N,E,PE,EE> : GraphView, INovaGraphView where N : GraphNode where E : NovaElement where PE : NovaElement where EE : NovaSwitcher
    {
        public virtual E linkedElement { get; set; }
        public EList<N> graphNodes { get; set; } = new();
        public EList<IGraphEdge> graphEdges { get; set; } = new();
        public new string name => linkedElement?.name;

        private N _firstNode;
        public virtual N firstNode
        {
            get => _firstNode;
            set
            {
                _firstNode?.unmarkStartNode();
                _firstNode = value;
                _firstNode?.markedAsStartNode();
            }
        }
        IList INovaGraphView.graphNodes { get => graphNodes; set => graphNodes = value as EList<N>; }
        IList INovaGraphView.graphEdges { get => graphEdges; set => graphEdges = value as EList<IGraphEdge>; }
        GraphNode INovaGraphView.firstNode { get => firstNode; set => firstNode = value as N; }
        INovaElement INovaGraphView.linkedElement { get => linkedElement; set => linkedElement = value as E; }

        protected NovaGraphView(E linkedElement,string name)
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnCanPaste;
            unserializeAndPaste = OnUnserializeAndPaste;
            graphViewChanged += OnGraphViewChanged;

            this.linkedElement = linkedElement;
        }
        protected virtual string getType()
        {
            return "[Default]";
        }
        public virtual string getActualName()
        {
            return getType() + " " + name;
        }

        public virtual N summonNewGraphNode(Vector2 pos)
        {
            return default;
        }

        public virtual N summonNewGraphNode(PE novaElement,Vector2 pos)
        {
            return default;
        }

        protected virtual Edge summonAndConnectEdge<Edge>(EE linkedSwitcher) where Edge : GraphEdge<PE,EE>,new()
        {
            var inputGraphNode = graphNodes?.Find(x => x.guid.Equals(linkedSwitcher.inputElement?.guid));
            var outputGraphNode = graphNodes?.Find(x => x.guid.Equals(linkedSwitcher.outputElement?.guid));

            if (inputGraphNode == null || outputGraphNode == null
                || inputGraphNode.inputContainer.childCount == 0 || inputGraphNode.outputContainer.childCount == 0
                || outputGraphNode.inputContainer.childCount == 0 || outputGraphNode.outputContainer.childCount == 0)
            {
                Debug.LogError("Can't find input or output node!");
                return default;
            }

            var inputPort = inputGraphNode.inputContainer?[0] as GraphPort<PE, EE>;
            var outputPort = outputGraphNode.outputContainer?[0] as GraphPort<PE, EE>;

            if (inputPort == null || outputPort == null)
            {
                Debug.LogError("Can't find port!");
                return default;
            }

            var edge = outputPort.ConnectTo<Edge>(inputPort, linkedSwitcher);

            return edge;
        }
        public virtual IGraphViewContext summonNewChildGraphContext(PE novaElement,Vector2 pos)
        {
            return default;
        }

        //Interface
        public virtual GraphNode summonNewGraphNode(INovaElement novaElement, Vector2 pos)
        {
            return summonNewGraphNode((PE)novaElement, pos);
        }

        //Interface
        public virtual IGraphEdge summonNewGraphEdge(INovaSwitcher switcher)
        {
            return summonNewGraphEdge((EE)switcher);
        }

        public virtual void addGraphEdge(IGraphEdge graphEdge,bool isLoading = false,bool autoSave = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toAdd) return;

            graphEdges?.Add(toAdd);
            AddElement(toAdd);
            graphEdge.linkedElement.parent = linkedElement;

            if (!isLoading)
            {
                update();
            }
        }
        public virtual void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toRemove) return;
            graphEdges?.Remove(toRemove);
            toRemove.input.DisconnectAll();
            toRemove.output.DisconnectAll();
            RemoveElement(toRemove);

            update();
        }
        public virtual void addGraphNode(GraphNode graphNode,bool isLoading = false,bool autoSave = true)
        {
            if (graphNode is not N toAdd) return;

            toAdd.linkedElement.parent = linkedElement;
            graphNodes?.Add(toAdd);
            AddElement(toAdd);
            setNodeUnpassable(toAdd);
            toAdd.update();

            if (!isLoading)
            {
                NovaWindow.RegisterContext(summonNewChildGraphContext((PE)toAdd.linkedElement, graphNode.pos));
                update();
            }
        }
        public virtual void removeGraphNode(GraphNode graphNode,bool autoSave = true)
        {
            if (graphNode is not N toRemove) return;

            graphNodes?.Remove(toRemove);
            RemoveElement(toRemove);

            update();
        }
        public virtual void selectGraphNode(string guid)
        {
            var toSelectGraphNode = getExistingGraphNode(guid);
            if (toSelectGraphNode == null) return;
            AddToSelection(toSelectGraphNode);
        }
        public virtual N getExistingGraphNode(string guid)
        {
            var elementsList = new List<GraphElement>(graphElements);
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is N node)
                {
                    if (node.guid.Equals(guid)) return node;
                }
            }
            return null;
        }
        protected virtual GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if(change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if(element is N graphNode)
                    {
                        removeGraphNode(graphNode,false);
                    }
                }
            }
            NovaWindow.UpdateContext();
            EditorFileManager.SaveGraphWindowData();
            return change;
        }
        protected virtual string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var elementsList = new List<GraphElement>(elements);
            var copiedData = new CopyPasteData<N>();
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is N graphNode)
                {
                    copiedData.data.Add(graphNode);
                    copiedData.pastePos = new Vector2(graphNode.GetPosition().x + 50,graphNode.GetPosition().y + 50); 
                }
            }
            return JsonUtility.ToJson(copiedData);
        }
        protected virtual void OnUnserializeAndPaste(string operationName, string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            var pasteData = JsonUtility.FromJson<CopyPasteData<N>>(data);
            if (pasteData == null || pasteData?.data?.Count == 0) return;

            ClearSelection();

            for (int i = 0; i < pasteData?.data?.Count; i++)
            {
                var nodeData = pasteData?.data?[i];
                if (nodeData is N graphNode)
                {
                    var n = summonNewGraphNode(pasteData.pastePos);
                    addGraphNode(n,false,false);
                    AddToSelection(n);
                }
            }
            EditorFileManager.SaveGraphWindowData();
        }
        protected virtual bool OnCanPaste(string data)
        {
            return !string.IsNullOrEmpty(data);
        }
        public virtual void update()
        {
            updateNodes();

            updateEdges();
        }
        protected virtual void updateNodes()
        {
            if (firstNode == null && graphNodes.Count != 0)
            {
                firstNode = graphNodes[0];
            }

            foreach (var graphNode in graphNodes)
            {
                setNodeUnpassable(graphNode);
                graphNode.update();
            }
            setNodePassable(firstNode);
        }
        protected virtual void updateEdges()
        {
            foreach (var graphEdge in graphEdges)
            {
                var inputGraphNode = getExistingGraphNode(graphEdge.linkedElement.inputElement.guid);
                var outputGraphNode = getExistingGraphNode(graphEdge.linkedElement.outputElement.guid);

                if (inputGraphNode == null || outputGraphNode == null) continue;

                if (!inputGraphNode.isPassable || !outputGraphNode.isPassable) setEdgeUnpassable((GraphEdge<PE, EE>)graphEdge);
                else setEdgePassable((GraphEdge<PE, EE>)graphEdge);
            }
        }
        protected virtual void setNodePassable(N graphNode)
        {
            if (graphNode == null) return;
            graphNode.style.opacity = 1f;
            graphNode.isPassable = true;
        }
        protected virtual void setNodeUnpassable(N graphNode)
        {
            if (graphNode == null) return;
            graphNode.style.opacity = 0.5f;
            graphNode.isPassable = false;
        }
        protected virtual void setEdgePassable(GraphEdge<PE,EE> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 1f;
        }
        protected virtual void setEdgeUnpassable(GraphEdge<PE,EE> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.5f;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            evt.menu.AppendAction("Add Graph Node", (action) =>
            {
                var e = summonNewGraphNode(graphMousePosition);
                addGraphNode(e);
            });
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            var portsList = new List<Port>(ports);
            for (int i = 0; i < portsList.Count; i++)
            {
                var port = portsList[i];
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            }

            return compatiblePorts;
        }
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);

            if (selectable is N node)
            {
                node.OnSelected();
            }
        }
    }
    public interface INovaGraphView
    {
        public INovaElement linkedElement { get; set; }
        public IList graphNodes { get; set; }
        public IList graphEdges { get; set; }
        public GraphNode firstNode { get; set; }
        public GraphNode summonNewGraphNode(INovaElement novaElement, Vector2 pos);
        public IGraphEdge summonNewGraphEdge(INovaSwitcher switcher);
        public void addGraphEdge(IGraphEdge graphEdge, bool isInit = false, bool autoSave = true);
        public void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true);
        public void addGraphNode(GraphNode graphNode, bool isInit = false, bool autoSave = true);
        public void removeGraphNode(GraphNode graphNode, bool autoSave = true);
        public void selectGraphNode(string guid);
        string getActualName();
        void update();
    }
}
[Serializable]
public class CopyPasteData<T> where T : GraphNode
{
    public List<T> data = new List<T>();
    public Vector2 pastePos;
}