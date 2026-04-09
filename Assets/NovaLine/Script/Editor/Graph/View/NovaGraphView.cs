﻿using System;
 using NovaLine.Script.Data.Edge;
 using NovaLine.Script.Data.NodeGraphView;
 using NovaLine.Script.Editor.Utils.Scope;
 using NovaLine.Script.Editor.Window.Context.Edge;
 using NovaLine.Script.Editor.Window.Context.GraphViewNode;
 using NovaLine.Script.Element.Switcher;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Graph.View
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;
    using NovaLine.Script.Editor.Graph.Node;
    using NovaLine.Script.Element;
    using NovaLine.Script.Editor.Graph.Edge;
    using NovaLine.Script.Editor.Graph.Port;
    using NovaLine.Script.Editor.Window.Command;
    using NovaLine.Script.Editor.Window;
    using static NovaLine.Script.Editor.Window.ContextRegistry;
    
    public abstract class NovaGraphView<TGraphNode,TLinkedElement,TGraphNodeElement,TSwitcherElement> : GraphView, INovaGraphView where TGraphNode : GraphNode where TLinkedElement : NovaElement where TGraphNodeElement : NovaElement where TSwitcherElement : NovaSwitcher
    {
        protected virtual Color themedColor => Color.black;
        public virtual string linkedElementGuid { get; set; }
        public virtual TLinkedElement linkedElement => FindElement(linkedElementGuid) as TLinkedElement;
        public virtual Dictionary<string, TGraphNode> graphNodes { get; } = new();
        public virtual Dictionary<string, IGraphEdge> graphEdges { get; } = new();
        public virtual NovaElementType type => linkedElement != null ? linkedElement.type : NovaElementType.NONE;
        public virtual Vector2 mousePos { get; set; }
        public System.Action OnRequestBackToParent { get; set; }
        
        public virtual TGraphNode firstNode
        {
            get => getExistingGraphNode(linkedElement?.firstChildGuid);
            set => linkedElement.firstChildGuid = value != null ? value.linkedElement.guid : "";
        }

        private Button _backButton;

        INovaElement INovaGraphView.linkedElement => linkedElement;
        IEnumerable INovaGraphView.graphNodes => graphNodes.Values;
        IEnumerable INovaGraphView.graphEdges => graphEdges.Values;

        protected NovaGraphView(string linkedElementGuid)
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

            this.linkedElementGuid = linkedElementGuid;
            
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

        public virtual TGraphNode summonNewGraphNode(Vector2 pos)
        {
            return default;
        }

        public virtual TGraphNode summonNewGraphNode(TGraphNodeElement element,Vector2 pos)
        {
            return default;
        }

        protected virtual TEdge summonAndConnectEdge<TEdge>(TSwitcherElement linkedSwitcher) where TEdge : GraphEdge<TGraphNodeElement,TSwitcherElement>,new()
        {
            try
            {
                var inputKey = linkedSwitcher?.inputElementGuid;
                var outputKey = linkedSwitcher?.outputElementGuid;
                TGraphNode inputGraphNode = inputKey != null && graphNodes.TryGetValue(inputKey, out var iNode) ? iNode : null;
                TGraphNode outputGraphNode = outputKey != null && graphNodes.TryGetValue(outputKey, out var oNode) ? oNode : null;

                if (inputGraphNode == null || outputGraphNode == null
                   || inputGraphNode.inputContainer.childCount == 0 || inputGraphNode.outputContainer.childCount == 0
                   || outputGraphNode.inputContainer.childCount == 0 || outputGraphNode.outputContainer.childCount == 0)
                {
                    throw new Exception("Can't find input or output node!");
                }

                var inputPort = inputGraphNode.inputContainer?[0] as GraphPort<TGraphNodeElement, TSwitcherElement>;
                var outputPort = outputGraphNode.outputContainer?[0] as GraphPort<TGraphNodeElement, TSwitcherElement>;

                if (inputPort == null || outputPort == null)
                {
                    throw new Exception("Can't find input or output port!");
                }

                var edge = outputPort.ConnectTo<TEdge>(inputPort, linkedSwitcher);

                return edge;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                linkedElement.switchersGuidList.Remove(linkedSwitcher?.guid);
                return null;
            }
        }
        public virtual IGraphViewNodeContext summonNewChildGraphViewNodeContext(NovaElement linkedElement,Vector2 pos)
        {
            return default;
        }

        public virtual IGraphViewNodeContext summonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return default;
        }

        public virtual EdgeContext summonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return default;
        }

        public virtual EdgeContext summonNewChildEdgeContext(IEdgeData linkedData)
        {
            return new EdgeContext(linkedData);
        }

        //Interface
        public virtual GraphNode summonNewGraphNode(NovaElement linkedElement, Vector2 pos)
        {
            return summonNewGraphNode((TGraphNodeElement)linkedElement, pos);
        }

        //Interface
        public virtual IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return default;
        }

        public virtual void addGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge is not GraphEdge<TGraphNodeElement, TSwitcherElement> toAdd) return;
            if (String.IsNullOrEmpty(toAdd.guid) || !graphEdges.TryAdd(toAdd.guid, toAdd)) return;
            
            AddElement(toAdd);
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge is not GraphEdge<TGraphNodeElement, TSwitcherElement> toRemove) return;
            if (String.IsNullOrEmpty(toRemove.guid) || !graphEdges.Remove(toRemove.guid)) return;

            toRemove.RemoveFromHierarchy();
            toRemove.input.Disconnect(toRemove);
            toRemove.output.Disconnect(toRemove);
            RemoveElement(toRemove);
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphEdge(string guid)
        {
            removeGraphEdge(getExistingGraphEdge(guid));
        }
        public virtual void addGraphNode(GraphNode graphNode)
        {
            if (graphNode is not TGraphNode toAdd) return;
            if (toAdd.guid == null || !graphNodes.TryAdd(toAdd.guid, toAdd)) return;

            if (String.IsNullOrEmpty(linkedElement?.firstChildGuid))
            {
                setFirstNode(graphNode, false);
            }

            AddElement(toAdd);
            setNodeUnpassable(toAdd);

            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }

        public virtual void removeGraphNode(GraphNode graphNode)
        {
            if (graphNode is not TGraphNode toRemove) return;

            if (toRemove.guid.Equals(linkedElement?.firstChildGuid))
            {
                if (graphNodes.Count > 1)
                {
                    resetFirstNode(toRemove);
                }
                else
                {
                    resetFirstNode();
                }
            }

            graphNodes.Remove(toRemove.guid);
            toRemove.RemoveFromHierarchy();
            RemoveElement(toRemove);

            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void removeGraphNode(string guid)
        {
            removeGraphNode(getExistingGraphNode(guid));
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
            if (graphEdge is not GraphEdge<TGraphNodeElement,TSwitcherElement> toSelect) return false;
            AddToSelection(toSelect);
            return true;
        }
        public virtual void setFirstNode(string guid,bool registerCommand = true)
        {
            setFirstNode(!String.IsNullOrEmpty(guid) ? getExistingGraphNode(guid) : null, registerCommand);
        }
        public virtual void setFirstNode(GraphNode graphNode, bool registerCommand = true)
        {
            if (registerCommand)
            {
                CommandRegistry.Register(new SetFirstNodeCommand(linkedElementGuid, type, firstNode == null ? null : firstNode.guid, graphNode.guid));
            }
            firstNode?.unmarkStartNode();
            firstNode = (TGraphNode)graphNode;
            firstNode?.markedAsStartNode();
        }
        public virtual TGraphNode getExistingGraphNode(string guid)
        {
            if (guid != null && graphNodes.TryGetValue(guid, out var node)) return node;
            return default;
        }
        public GraphNode getExistingGraphNode(string guid,int inInterface)
        {
            return getExistingGraphNode(guid);
        }
        public virtual GraphEdge<TGraphNodeElement,TSwitcherElement> getExistingGraphEdge(string guid)
        {
            if (guid != null && graphEdges.TryGetValue(guid, out var edge) && edge is GraphEdge<TGraphNodeElement, TSwitcherElement> typedEdge) return typedEdge;
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
                        if (element is GraphEdge<TGraphNodeElement,TSwitcherElement> graphEdge)
                        {
                            removeGraphEdgeByHand(graphEdge);
                        }
                    }
                    foreach (var element in change.elementsToRemove)
                    {
                        if (element is TGraphNode graphNode)
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
            if (String.IsNullOrEmpty(linkedElement?.firstChildGuid))
            {
                resetFirstNode();
            }

            foreach (var graphNode in graphNodes.Values)
            {
                setNodeUnpassable(graphNode);
                graphNode.update();
            }
            setNodePassable(firstNode);
        }
        protected virtual void updateEdges()
        {
            var edgesToCheck = new List<IGraphEdge>(graphEdges.Values);
            foreach (var graphEdge in edgesToCheck)
            {
                var inputGraphNode = getExistingGraphNode(graphEdge.linkedElement.inputElementGuid);
                var outputGraphNode = getExistingGraphNode(graphEdge.linkedElement.outputElementGuid);

                if (inputGraphNode == null || outputGraphNode == null)
                {
                    removeGraphEdgeByHand(graphEdge);
                    continue;
                }

                if (!inputGraphNode.isPassable || !outputGraphNode.isPassable) setEdgeUnpassable((GraphEdge<TGraphNodeElement, TSwitcherElement>)graphEdge);
                else setEdgePassable((GraphEdge<TGraphNodeElement, TSwitcherElement>)graphEdge);
            }
        }
        protected virtual void setNodePassable(TGraphNode graphNode)
        {
            if (graphNode == null || graphNode.isPassable) return;
            graphNode.style.opacity = 1f;
            graphNode.isPassable = true;
            
            if (graphNode.linkedElement is not TGraphNodeElement element || element.switchersGuidList == null || element.switchersGuidList.Count == 0) return;
            
            foreach (var nodeSwitcherGuid in element.switchersGuidList)
            {
                var nodeSwitcher = FindElement(nodeSwitcherGuid) as NovaSwitcher;
                if (nodeSwitcher == null || String.IsNullOrEmpty(nodeSwitcher.inputElementGuid) || String.IsNullOrEmpty(nodeSwitcher.outputElementGuid)) continue;
                setNodePassable(getExistingGraphNode(nodeSwitcher.inputElementGuid));
            }
        }
        protected virtual void setNodeUnpassable(TGraphNode graphNode)
        {
            if (graphNode == null || !graphNode.isPassable) return;
            graphNode.style.opacity = 0.5f;
            graphNode.isPassable = false;

            if (graphNode.linkedElement is not TGraphNodeElement element || element.switchersGuidList == null || element.switchersGuidList.Count == 0) return;

            foreach (var nodeSwitcherGuid in element.switchersGuidList)
            {
                var nodeSwitcher = FindElement(nodeSwitcherGuid) as NovaSwitcher;
                if (nodeSwitcher == null || String.IsNullOrEmpty(nodeSwitcher.inputElementGuid) || String.IsNullOrEmpty(nodeSwitcher.outputElementGuid)) continue;
                setNodeUnpassable(getExistingGraphNode(nodeSwitcher.inputElementGuid));
            }
        }
        protected virtual void setEdgePassable(GraphEdge<TGraphNodeElement,TSwitcherElement> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 1f;
        }
        protected virtual void setEdgeUnpassable(GraphEdge<TGraphNodeElement,TSwitcherElement> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.5f;
        }

        public virtual void addGraphNodeByHand(GraphNode graphNode,Vector2 pos)
        {
            //Register context to create data
            var newGraphNodeContext = summonNewChildGraphViewNodeContext((TGraphNodeElement)graphNode.linkedElement, pos);
            RegisterContext(newGraphNodeContext);
                
            //Recording command
            var linkedData = newGraphNodeContext.linkedData;
            if(linkedData != null) CommandRegistry.Register(new AddNodeCommand(linkedElementGuid, type, linkedData.strongCopy() as IGraphViewNodeData));
            
            //In the end: Add node to graph view
            graphNode.linkedElement.setParent(linkedElement);
            addGraphNode(graphNode);
        }
        public virtual void removeGraphNodeByHand(GraphNode graphNode)
        {
            //First: remove node from graph view
            graphNode.linkedElement.setParent(null);
            removeGraphNode(graphNode);
                
            //Recording command
            var graphNodeContext = GetContext(graphNode.guid, graphNode.type);
            var linkedData = graphNodeContext?.linkedData;
            if(linkedData != null) CommandRegistry.Register(new RemoveNodeCommand(linkedElementGuid, type, linkedData.strongCopy() as IGraphViewNodeData));
                
            //Unregister context
            UnregisterContext(graphNodeContext);
        }

        public virtual void addGraphEdgeByHand(IGraphEdge graphEdge)
        {
            var newEdgeContext = summonNewChildEdgeContext(graphEdge.linkedElement);
            RegisterContext(newEdgeContext);
            
            graphEdge.linkedElement.setParent(linkedElement);
            
            //Put edge to graph view
            addGraphEdge(graphEdge);
            
            //Recording command
            var linkedData = newEdgeContext.linkedData;
            if(linkedData != null) CommandRegistry.Register(new AddEdgeCommand(linkedElementGuid, type, linkedData.strongCopy() as IEdgeData));
        }
        public virtual void removeGraphEdgeByHand(IGraphEdge graphEdge)
        {
            graphEdge.linkedElement.setParent(null);
            
            //Remove edge from graph view
            removeGraphEdge(graphEdge);
            
            //Recording command
            var edgeContext = GetContext(graphEdge.guid, NovaElementType.SWITCHER);
            var linkedData = edgeContext?.linkedData;
            if(linkedData != null) CommandRegistry.Register(new RemoveEdgeCommand(linkedElementGuid, type, linkedData.strongCopy() as IEdgeData));
            
            UnregisterContext(edgeContext);
        }
        
        public virtual void addGraphNodeByCommand(IGraphViewNodeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.guid) is not TGraphNodeElement addElement) return;
            RegisterContext(summonNewChildGraphViewNodeContext(linkedData));
            
            addElement.setParent(linkedElement);
            
            var graphNode = summonNewGraphNode(addElement, linkedData.pos);
            addGraphNode(graphNode);
            
            //Not need to record command.
        }
        public virtual void removeGraphNodeByCommand(IGraphViewNodeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.guid) is not TGraphNodeElement removeElement) return;
            
            removeGraphNode(linkedData.guid);
            
            linkedData.linkedElement.setParent(null);
            
            UnregisterContext(removeElement.guid, removeElement.type);
            
            //Not need to record command.
        }

        public virtual void addGraphEdgeByCommand(IEdgeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.guid) is not TSwitcherElement switcherElement) return;
            
            RegisterContext(summonNewChildEdgeContext(linkedData));
            
            switcherElement.setParent(linkedElement);
            
            var graphEdge = summonNewGraphEdge(switcherElement);
            addGraphEdge(graphEdge);
        }

        public virtual void removeGraphEdgeByCommand(IEdgeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.guid) is not TSwitcherElement switcherElement) return;
            
            switcherElement.setParent(null);
            
            removeGraphEdge(linkedData.guid);
            
            UnregisterContext(linkedData.guid, switcherElement.type);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            evt.menu.AppendAction("Add Graph Node", _ =>
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
        private void resetFirstNode(TGraphNode excludeNode = null,bool registerCommand = false)
        {
            if (excludeNode == null)
            {
                setFirstNode("",registerCommand);
                return;
            }
            foreach (var node in graphNodes.Values)
            {
                if (node.guid == excludeNode.guid) continue;
                setFirstNode(node, registerCommand);
                return;
            }
        } 
        private void createFloatingBackButton()
        {
            _backButton = new Button(() => { OnRequestBackToParent?.Invoke(); })
            {
                text = "Back",
                style =
                {
                    position = Position.Absolute,
                    top = 15,
                    left = 15
                }
            };

            const float buttonHeight = 28f;
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
            
            _backButton.RegisterCallback<MouseEnterEvent>(_ => {
                _backButton.style.backgroundColor = new StyleColor(themedColor);
            });
            _backButton.RegisterCallback<MouseLeaveEvent>(_ => {
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
        string linkedElementGuid { get; set; }
        INovaElement linkedElement { get;}
        IEnumerable graphNodes { get; }
        IEnumerable graphEdges { get; }
        Vector2 mousePos { get;}
        System.Action OnRequestBackToParent { get; set; }
        GraphNode summonNewGraphNode(NovaElement linkedElement, Vector2 pos);
        IGraphEdge summonNewGraphEdge(NovaSwitcher linkedSwitcher);
        IGraphViewNodeContext summonNewChildGraphViewNodeContext(NovaElement linkedElement,Vector2 pos);
        IGraphViewNodeContext summonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData);
        EdgeContext summonNewChildEdgeContext(NovaSwitcher linkedSwitcher);
        EdgeContext summonNewChildEdgeContext(IEdgeData linkedData);
        void addGraphEdge(IGraphEdge graphEdge);
        void removeGraphEdge(IGraphEdge graphEdge);
        void removeGraphEdge(string guid);
        void addGraphNode(GraphNode graphNode);
        void removeGraphNode(GraphNode graphNode);
        void removeGraphNode(string guid);
        GraphNode getExistingGraphNode(string guid,int inInterface);
        void moveGraphNode(string guid, Vector2 newPos, bool registerCommand = true);
        bool selectGraphNode(string guid);
        bool selectGraphNode(GraphNode graphNode);
        bool selectGraphEdge(string guid);
        bool selectGraphEdge(IGraphEdge graphEdge);
        void setFirstNode(string guid, bool registerCommand = true);
        void setFirstNode(GraphNode graphNode, bool registerCommand = true);
        void addGraphNodeByHand(GraphNode graphNode, Vector2 pos);
        void removeGraphNodeByHand(GraphNode graphNode);
        void addGraphEdgeByHand(IGraphEdge graphEdge);
        void removeGraphEdgeByHand(IGraphEdge graphEdge);
        void addGraphNodeByCommand(IGraphViewNodeData linkedData);
        void removeGraphNodeByCommand(IGraphViewNodeData linkedData);
        void addGraphEdgeByCommand(IEdgeData linkedData);
        void removeGraphEdgeByCommand(IEdgeData linkedData);
        void setBackButtonVisible(bool isVisible);
        string getActualName();
        void update();
    }
}
