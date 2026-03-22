using System.Collections.Generic;
using System;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;
using static NovaWindow;
using NovaLine.Element;

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
    using NovaLine.Editor.Utils.Ext;
    using NovaLine.Editor.Window.Command;
    using NovaLine.Editor.Window;
    using Unity.VisualScripting.YamlDotNet.Core.Tokens;

    public abstract class NovaGraphView<N,E,PE,EE> : GraphView, INovaGraphView where N : GraphNode where E : NovaElement where PE : NovaElement where EE : NovaSwitcher
    {
        public virtual E linkedElement { get; set; }
        public virtual EList<N> graphNodes { get; set; } = new();
        public virtual EList<IGraphEdge> graphEdges { get; set; } = new();
        public virtual NovaElementType type => linkedElement != null ? linkedElement.type : NovaElementType.NONE;
        public new string name => linkedElement?.name;

        private N _firstNode;
        public virtual N firstNode
        {
            get => _firstNode;
            set
            {
                _firstNode = value;
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

            serializeGraphElements = copyGraphElement;
            canPasteSerializedData = onCanPaste;
            unserializeAndPaste = pasteGraphElement;
            graphViewChanged += onGraphViewChanged;

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

        public virtual void addGraphEdge(IGraphEdge graphEdge,bool isLoading = false,bool autoSave = true, bool registerCommand = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toAdd) return;

            graphEdges?.Add(toAdd);
            AddElement(toAdd);
            graphEdge.linkedElement.parent = linkedElement;

            if (!isLoading)
            {
                update();
            }

            if (registerCommand)
            {
                CommandRegistry.Register(new AddEdgeCommand(linkedElement.guid, type, graphEdge));
            }
        }
        public virtual void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true, bool registerCommand = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toRemove) return;

            graphEdges?.Remove(toRemove);
            toRemove.RemoveFromHierarchy();
            toRemove.input.Disconnect(toRemove,registerCommand);
            toRemove.output.Disconnect(toRemove, registerCommand);
            RemoveElement(toRemove);

            update();

            if (registerCommand)
            {
                CommandRegistry.Register(new RemoveEdgeCommand(linkedElement.guid, type, graphEdge));
            }
        }
        public virtual void removeGraphEdge(string guid, bool autoSave = true, bool registerCommand = true)
        {
            removeGraphEdge(getExistingGraphEdge(guid), autoSave, registerCommand);
        }
        public virtual void addGraphNode(GraphNode graphNode,bool isLoading = false,bool autoSave = true,bool registerCommand = true)
        {
            if (graphNode is not N toAdd) return;

            toAdd.linkedElement.parent = linkedElement;
            graphNodes?.Add(toAdd);
            AddElement(toAdd);
            setNodeUnpassable(toAdd);
            toAdd.update();

            if (!isLoading)
            {
                RegisterContext(summonNewChildGraphContext((PE)toAdd.linkedElement, graphNode.pos));
                update();
            }

            if (registerCommand)
            {
                CommandRegistry.Register(new AddNodeCommand(linkedElement.guid, type, graphNode));
            }
        }
        public virtual void removeGraphNode(GraphNode graphNode,bool autoSave = true, bool registerCommand = true)
        {
            if (graphNode is not N toRemove) return;

            if (toRemove.guid.Equals(firstNode?.guid))
            {
                if(graphNodes.Count > 1)
                {
                    var index = graphNodes.IndexOf(toRemove);
                    resetFirstNode(index,true);
                }
            }

            graphNodes?.Remove(toRemove);
            toRemove.RemoveFromHierarchy();
            RemoveElement(toRemove);

            update();

            if (registerCommand)
            {
                CommandRegistry.Register(new RemoveNodeCommand(linkedElement.guid, type, graphNode));
            }
        }
        public virtual void removeGraphNode(string guid, bool autoSave = true, bool registerCommand = true)
        {
            removeGraphNode(getExistingGraphNode(guid),autoSave,registerCommand);
        }
        public virtual void moveGraphNode(string guid,Vector2 newPos,bool registerCommand = true)
        {
            var graphNode = getExistingGraphNode(guid);
            if(graphNode != null)
            {
                graphNode.SetPosition(newPos, registerCommand);
            }
        }
        public virtual bool selectGraphNode(string guid)
        {
            return selectGraphNode(getExistingGraphNode(guid));
        }
        public bool selectGraphNode(GraphNode graphNode)
        {
            if (graphNode == null) return false;
            AddToSelection(graphNode);
            return true;
        }

        public virtual bool selectGraphEdge(string guid)
        {
            var toSelectGraphEdge = getExistingGraphEdge(guid);
            if (toSelectGraphEdge == null) return false;
            AddToSelection(toSelectGraphEdge);
            return true;
        }
        public virtual void setFirstNode(string guid,bool registerCommand = true)
        {
            setFirstNode(getExistingGraphNode(guid), registerCommand);
        }
        public virtual void setFirstNode(GraphNode graphNode, bool registerCommand = true)
        {
            if (registerCommand)
            {
                CommandRegistry.Register(new SetFirstNodeCommand(linkedElement.guid, type, firstNode == null ? null : firstNode.guid, graphNode.guid));
            }
            firstNode?.unmarkStartNode();
            firstNode = (N)graphNode;
            firstNode?.markedAsStartNode();
        }
        public virtual N getExistingGraphNode(string guid)
        {
            return (N)getExistingGraphNode(guid,0);
        }
        public GraphNode getExistingGraphNode(string guid,int inInterface)
        {
            var elementsList = new List<GraphElement>(graphElements);
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is GraphNode node)
                {
                    if (node.guid.Equals(guid)) return node;
                }
            }
            return null;
        }
        public virtual GraphEdge<PE,EE> getExistingGraphEdge(string guid)
        {
            var elementsList = new List<GraphElement>(graphElements);
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is GraphEdge<PE, EE> edge)
                {
                    if (edge.guid.Equals(guid)) return edge;
                }
            }
            return null;
        }
        protected virtual GraphViewChange onGraphViewChanged(GraphViewChange change)
        {
            if(change.elementsToRemove != null)
            {
                using (new CommandScope())
                {
                    foreach (var element in change.elementsToRemove)
                    {
                        if (element is N graphNode)
                        {
                            removeGraphNode(graphNode, false);
                        }
                    }
                }
            }
            UpdateContext();
            EditorFileManager.SaveGraphWindowData();
            return change;
        }
        protected virtual string copyGraphElement(IEnumerable<GraphElement> elements)
        {
            var elementsList = new List<GraphElement>(elements);
            var copiedData = new CopyPasteData<PE>();
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is N graphNode)
                {
                    copiedData.elementData.Add((PE)graphNode.linkedElement);
                    copiedData.pastePos = new Vector2(graphNode.GetPosition().x + 50,graphNode.GetPosition().y + 50); 
                }
            }
            return JsonUtility.ToJson(copiedData);
        }
        protected virtual void pasteGraphElement(string operationName, string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            var pasteData = JsonUtility.FromJson<CopyPasteData<PE>>(data);
            if (pasteData == null || pasteData.elementData?.Count == 0) return;

            ClearSelection();

            using (new CommandScope())
            {
                for (int i = 0; i < pasteData.elementData.Count; i++)
                {
                    var nodeElement = pasteData.elementData[i];
                    nodeElement.guid = Guid.NewGuid().ToString();
                    var n = summonNewGraphNode(nodeElement, pasteData.pastePos);
                    addGraphNode(n, false, false);
                    AddToSelection(n);
                }
            }

            EditorFileManager.SaveGraphWindowData();
        }
        protected virtual bool onCanPaste(string data)
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
            if (firstNode == null)
            {
                resetFirstNode();
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
        private void resetFirstNode(int index = 0,bool registerCommand = false)
        {
            if(graphNodes.Count != 0)
            {
                setFirstNode(graphNodes[(index + 1) % graphNodes.Count], registerCommand);
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
        public void addGraphEdge(IGraphEdge graphEdge, bool isLoading = false, bool autoSave = true, bool registerCommand = true);
        public void removeGraphEdge(IGraphEdge graphEdge, bool autoSave = true, bool registerCommand = true);
        public void removeGraphEdge(string guid, bool autoSave = true, bool registerCommand = true);
        public void addGraphNode(GraphNode graphNode, bool isLoading = false, bool autoSave = true, bool registerCommand = true);
        public void removeGraphNode(GraphNode graphNode, bool autoSave = true, bool registerCommand = true);
        public void removeGraphNode(string guid, bool autoSave = true, bool registerCommand = true);
        public GraphNode getExistingGraphNode(string guid,int inInterface);
        public void moveGraphNode(string guid, Vector2 newPos, bool registerCommand = true);
        public bool selectGraphNode(string guid);
        public bool selectGraphNode(GraphNode graphNode);
        public bool selectGraphEdge(string guid);
        public void setFirstNode(string guid, bool registerCommand = true);
        public void setFirstNode(GraphNode graphNode, bool registerCommand = true);
        string getActualName();
        void update();
    }
}
[Serializable]
public class CopyPasteData<PE> where PE : NovaElement
{
    public List<PE> elementData = new();
    public Vector2 pastePos;
}