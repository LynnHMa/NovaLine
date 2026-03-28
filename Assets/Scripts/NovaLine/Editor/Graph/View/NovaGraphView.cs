using NovaLine.Editor.Utils.Scope;
using NovaLine.Element.Switcher;

namespace NovaLine.Editor.Graph.View
{
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Element;
    using NovaLine.Editor.Graph.Edge;
    using NovaLine.Editor.Graph.Port;
    using System.Collections;
    using NovaLine.Editor.Window.Context;
    using NovaLine.Editor.Utils.Ext;
    using NovaLine.Editor.Window.Command;
    using NovaLine.Editor.Window;
    using static NovaLine.Editor.Window.WindowContextRegistry;
    
    public abstract class NovaGraphView<N,E,PE,EE> : GraphView, INovaGraphView where N : GraphNode where E : NovaElement where PE : NovaElement where EE : NovaSwitcher
    {
        protected virtual Color themedColor => Color.black;
        public virtual E linkedElement { get; set; }
        public virtual EList<N> graphNodes { get; set; } = new();
        public virtual EList<IGraphEdge> graphEdges { get; set; } = new();
        public virtual NovaElementType type => linkedElement != null ? linkedElement.type : NovaElementType.NONE;
        public virtual Vector2 mousePos { get; set; }
        public System.Action OnRequestBackToParent { get; set; }

        private N _firstNode;
        public virtual N firstNode
        {
            get => _firstNode;
            set
            {
                _firstNode = value;
                linkedElement.firstChild = (PE)value.linkedElement;
            }
        }

        private Button _backButton;

        IList INovaGraphView.graphNodes { get => graphNodes; set => graphNodes = value as EList<N>; }
        IList INovaGraphView.graphEdges { get => graphEdges; set => graphEdges = value as EList<IGraphEdge>; }
        GraphNode INovaGraphView.firstNode { get => firstNode; set => firstNode = value as N; }
        INovaElement INovaGraphView.linkedElement { get => linkedElement; set => linkedElement = value as E; }

        protected NovaGraphView(E linkedElement)
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            serializeGraphElements = CopyPasteHelper.Copy;
            canPasteSerializedData = CopyPasteHelper.OnCanPaste;
            unserializeAndPaste = CopyPasteHelper.Paste;
            graphViewChanged += onGraphViewChanged;

            this.linkedElement = linkedElement;
            
            RegisterCallback<MouseMoveEvent>(evt =>
            {
                mousePos = contentViewContainer.WorldToLocal(evt.mousePosition);
            });
            
            createFloatingBackButton();
        }
        public virtual string getActualName()
        {
            return linkedElement?.getActualName();
        }

        public virtual N summonNewGraphNode(Vector2 pos)
        {
            return default;
        }

        public virtual N summonNewGraphNode(PE novaElement,Vector2 pos)
        {
            return default;
        }

        protected virtual TEdge summonAndConnectEdge<TEdge>(EE linkedSwitcher) where TEdge : GraphEdge<PE,EE>,new()
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

            var edge = outputPort.ConnectTo<TEdge>(inputPort, linkedSwitcher);

            return edge;
        }
        public virtual IGraphViewContext summonNewChildGraphContext(NovaElement novaElement,Vector2 pos)
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

        public virtual void addGraphEdge(IGraphEdge graphEdge,bool registerCommand = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toAdd) return;

            graphEdges?.Add(toAdd);
            AddElement(toAdd);
            graphEdge.linkedElement.parent = linkedElement;

            if (registerCommand)
            {
                CommandRegistry.Register(new AddEdgeCommand(linkedElement.guid, type, graphEdge));
            }
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphEdge(IGraphEdge graphEdge, bool registerCommand = true)
        {
            if (graphEdge is not GraphEdge<PE, EE> toRemove) return;

            graphEdges?.Remove(toRemove);
            toRemove.RemoveFromHierarchy();
            toRemove.input.Disconnect(toRemove,registerCommand);
            toRemove.output.Disconnect(toRemove, registerCommand);
            RemoveElement(toRemove);

            if (registerCommand)
            {
                CommandRegistry.Register(new RemoveEdgeCommand(linkedElement.guid, type, graphEdge));
            }
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphEdge(string guid, bool registerCommand = true)
        {
            removeGraphEdge(getExistingGraphEdge(guid), registerCommand);
        }
        public virtual void addGraphNode(GraphNode graphNode,bool registerCommand = true)
        {
            if (graphNode is not N toAdd) return;
            
            linkedElement.addChild(toAdd.linkedElement);
            graphNodes?.Add(toAdd);
            AddElement(toAdd);
            setNodeUnpassable(toAdd);

            if (registerCommand)
            {
                CommandRegistry.Register(new AddNodeCommand(linkedElement.guid, type, toAdd));
            }
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphNode(GraphNode graphNode,bool registerCommand = true)
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
            
            linkedElement.removeChild(toRemove.linkedElement);
            graphNodes?.Remove(toRemove);
            
            toRemove.RemoveFromHierarchy();
            RemoveElement(toRemove);

            if (registerCommand)
            {
                CommandRegistry.Register(new RemoveNodeCommand(linkedElement.guid, type, toRemove));
            }
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphNode(string guid, bool registerCommand = true)
        {
            removeGraphNode(getExistingGraphNode(guid),registerCommand);
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
        public virtual bool selectGraphNode(GraphNode graphNode)
        {
            if (graphNode == null) return false;
            AddToSelection(graphNode);
            return true;
        }
        public virtual bool selectGraphEdge(string guid)
        {
            return selectGraphEdge(getExistingGraphEdge(guid));
        }

        public bool selectGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge == null || graphEdge is not GraphEdge<PE,EE> toSelect) return false;
            AddToSelection(toSelect);
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
                using (new SaveScope())
                using (new CommandScope())
                using (new UpdateScope())
                {
                    foreach (var element in change.elementsToRemove)
                    {
                        if (element is N graphNode)
                        {
                            removeGraphNodeByHand(graphNode);
                        }
                    }
                }
            }
            return change;
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
            
            if (graphNode.linkedElement is not PE element || element.switchers == null || element.switchers.Count == 0) return;
            
            foreach (var nodeSwitcher in element.switchers)
            {
                if (nodeSwitcher.inputElement == null || nodeSwitcher.outputElement == null) continue;
                setNodePassable(getExistingGraphNode(nodeSwitcher.inputElement.guid));
            }
        }
        protected virtual void setNodeUnpassable(N graphNode)
        {
            if (graphNode == null || !graphNode.isPassable) return;
            graphNode.style.opacity = 0.5f;
            graphNode.isPassable = false;

            if (graphNode.linkedElement is not PE element || element.switchers == null || element.switchers.Count == 0) return;

            foreach (var switcher in element.switchers)
            {
                if (switcher.inputElement == null || switcher.outputElement == null) continue;
                setNodeUnpassable(getExistingGraphNode(switcher.inputElement.guid));
            }
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

        public virtual void addGraphNodeByHand(GraphNode graphNode,Vector2 pos)
        {
            addGraphNode(graphNode);
            RegisterContext(summonNewChildGraphContext((PE)graphNode.linkedElement, pos));
        }

        public virtual void removeGraphNodeByHand(GraphNode graphNode)
        {
            removeGraphNode(graphNode);
            UnregisterContext(graphNode.guid, graphNode.type);
        }

        public virtual void addGraphNodeByCommand(NovaElement linkedElement,Vector2 pos)
        {
            var graphNode = summonNewGraphNode(linkedElement, pos);
            addGraphNode(graphNode,false);
            RegisterContext(summonNewChildGraphContext((PE)linkedElement, pos));
        }

        public virtual void removeGraphNodeByCommand(NovaElement linkedElement)
        {
            removeGraphNode(linkedElement.guid,false);
            UnregisterContext(linkedElement.guid, linkedElement.type);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            evt.menu.AppendAction("Add Graph Node", (action) =>
            {
                var e = summonNewGraphNode(graphMousePosition);
                addGraphNodeByHand(e,graphMousePosition);
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
        private void resetFirstNode(int index = 0,bool registerCommand = false)
        {
            if(graphNodes.Count != 0)
            {
                setFirstNode(graphNodes[(index + 1) % graphNodes.Count], registerCommand);
            }
        } 
        private void createFloatingBackButton()
        {
            _backButton = new Button(() => { OnRequestBackToParent?.Invoke(); })
            {
                text = "Back"
            };

            _backButton.style.position = Position.Absolute;
            _backButton.style.top = 15;
            _backButton.style.left = 15;

            float buttonHeight = 28f;
            _backButton.style.height = buttonHeight;
            _backButton.style.paddingLeft = 8;
            _backButton.style.paddingRight = 12;

            _backButton.style.borderTopLeftRadius = buttonHeight / 2f;
            _backButton.style.borderBottomLeftRadius = buttonHeight / 2f;
            _backButton.style.borderTopRightRadius = 4;
            _backButton.style.borderBottomRightRadius = 4;
            
            _backButton.style.backgroundColor = new StyleColor(themedColor);
            _backButton.style.color = new StyleColor(Color.white);
            _backButton.style.fontSize = 13;
            _backButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            _backButton.style.borderTopWidth = 1;
            _backButton.style.borderBottomWidth = 1;
            _backButton.style.borderLeftWidth = 1;
            _backButton.style.borderRightWidth = 1;
            
            _backButton.RegisterCallback<MouseEnterEvent>(e => {
                _backButton.style.backgroundColor = new StyleColor(themedColor);
            });
            _backButton.RegisterCallback<MouseLeaveEvent>(e => {
                _backButton.style.backgroundColor = new StyleColor(themedColor);
            });

            _backButton.style.display = DisplayStyle.None;
            Add(_backButton);
        }
        public void setBackButtonVisible(bool isVisible)
        {
            _backButton.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    public interface INovaGraphView
    {
        public INovaElement linkedElement { get; set; }
        public IList graphNodes { get; set; }
        public IList graphEdges { get; set; }
        public GraphNode firstNode { get; set; }
        public Vector2 mousePos { get; set; }
        public System.Action OnRequestBackToParent { get; set; }
        public GraphNode summonNewGraphNode(INovaElement novaElement, Vector2 pos);
        public IGraphEdge summonNewGraphEdge(INovaSwitcher switcher);
        public IGraphViewContext summonNewChildGraphContext(NovaElement novaElement,Vector2 pos);
        public void addGraphEdge(IGraphEdge graphEdge, bool registerCommand = true);
        public void removeGraphEdge(IGraphEdge graphEdge, bool registerCommand = true);
        public void removeGraphEdge(string guid, bool registerCommand = true);
        public void addGraphNode(GraphNode graphNode, bool registerCommand = true);
        public void removeGraphNode(GraphNode graphNode, bool registerCommand = true);
        public void removeGraphNode(string guid, bool registerCommand = true);
        public GraphNode getExistingGraphNode(string guid,int inInterface);
        public void moveGraphNode(string guid, Vector2 newPos, bool registerCommand = true);
        public bool selectGraphNode(string guid);
        public bool selectGraphNode(GraphNode graphNode);
        public bool selectGraphEdge(string guid);
        public bool selectGraphEdge(IGraphEdge graphEdge);
        public void setFirstNode(string guid, bool registerCommand = true);
        public void setFirstNode(GraphNode graphNode, bool registerCommand = true);
        public void addGraphNodeByHand(GraphNode graphNode, Vector2 pos);
        public void removeGraphNodeByHand(GraphNode graphNode);
        public void addGraphNodeByCommand(NovaElement linkedElement, Vector2 pos);
        public void removeGraphNodeByCommand(NovaElement linkedElement);
        public void setBackButtonVisible(bool isVisible);
        string getActualName();
        void update();
    }
}
