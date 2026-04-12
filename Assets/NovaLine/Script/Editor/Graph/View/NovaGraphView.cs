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
        private Button _backButton;
        
        protected virtual Color ThemedColor => Color.black;
        public virtual string LinkedElementGuid { get; set; }
        public virtual TLinkedElement LinkedElement => FindElement(LinkedElementGuid) as TLinkedElement;
        public virtual Dictionary<string, TGraphNode> GraphNodes { get; } = new();
        public virtual Dictionary<string, IGraphEdge> GraphEdges { get; } = new();
        public virtual NovaElementType Type => LinkedElement != null ? LinkedElement.Type : NovaElementType.NONE;
        public virtual Vector2 MousePos { get; set; }
        public System.Action OnRequestBackToParent { get; set; }
        public virtual TGraphNode FirstNode
        {
            get => GetExistingGraphNode(LinkedElement?.FirstChildGuid);
            set => LinkedElement.FirstChildGuid = value != null ? value.linkedElement.Guid : "";
        }

        INovaElement INovaGraphView.LinkedElement => LinkedElement;
        IEnumerable INovaGraphView.GraphNodes => GraphNodes.Values;
        IEnumerable INovaGraphView.GraphEdges => GraphEdges.Values;

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
            graphViewChanged += OnGraphViewChanged;

            this.LinkedElementGuid = linkedElementGuid;
            
            RegisterCallback<MouseMoveEvent>(evt =>
            {
                MousePos = contentViewContainer.WorldToLocal(evt.mousePosition);
            });
            
            CreateFloatingBackButton();
        }
        public virtual string GetActualName()
        {
            return LinkedElement?.GetActualName();
        }

        public virtual TGraphNode SummonNewGraphNode(Vector2 pos)
        {
            return default;
        }

        public virtual TGraphNode SummonNewGraphNode(TGraphNodeElement element,Vector2 pos)
        {
            return default;
        }

        protected virtual TEdge SummonAndConnectEdge<TEdge>(TSwitcherElement linkedSwitcher) where TEdge : GraphEdge<TGraphNodeElement,TSwitcherElement>,new()
        {
            try
            {
                var inputKey = linkedSwitcher?.inputElementGuid;
                var outputKey = linkedSwitcher?.outputElementGuid;
                TGraphNode inputGraphNode = inputKey != null && GraphNodes.TryGetValue(inputKey, out var iNode) ? iNode : null;
                TGraphNode outputGraphNode = outputKey != null && GraphNodes.TryGetValue(outputKey, out var oNode) ? oNode : null;

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
                LinkedElement.SwitchersGuidList.Remove(linkedSwitcher?.Guid);
                return null;
            }
        }
        public virtual IGraphViewNodeContext SummonNewChildGraphViewNodeContext(NovaElement linkedElement,Vector2 pos)
        {
            return default;
        }

        public virtual IGraphViewNodeContext SummonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData)
        {
            return default;
        }

        public virtual EdgeContext SummonNewChildEdgeContext(NovaSwitcher linkedSwitcher)
        {
            return default;
        }

        public virtual EdgeContext SummonNewChildEdgeContext(IEdgeData linkedData)
        {
            return new EdgeContext(linkedData);
        }

        //Interface
        public virtual GraphNode SummonNewGraphNode(NovaElement linkedElement, Vector2 pos)
        {
            return SummonNewGraphNode((TGraphNodeElement)linkedElement, pos);
        }

        //Interface
        public virtual IGraphEdge SummonNewGraphEdge(NovaSwitcher linkedSwitcher)
        {
            return default;
        }

        public virtual void AddGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge is not GraphEdge<TGraphNodeElement, TSwitcherElement> toAdd) return;
            if (String.IsNullOrEmpty(toAdd.Guid) || !GraphEdges.TryAdd(toAdd.Guid, toAdd)) return;
            
            AddElement(toAdd);
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void RemoveGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge is not GraphEdge<TGraphNodeElement, TSwitcherElement> toRemove) return;
            if (String.IsNullOrEmpty(toRemove.Guid) || !GraphEdges.Remove(toRemove.Guid)) return;

            toRemove.RemoveFromHierarchy();
            toRemove.Input.Disconnect(toRemove);
            toRemove.Output.Disconnect(toRemove);
            RemoveElement(toRemove);
            
            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void RemoveGraphEdge(string guid)
        {
            RemoveGraphEdge(getExistingGraphEdge(guid));
        }
        public virtual void AddGraphNode(GraphNode graphNode)
        {
            if (graphNode is not TGraphNode toAdd) return;
            if (toAdd.guid == null || !GraphNodes.TryAdd(toAdd.guid, toAdd)) return;

            if (String.IsNullOrEmpty(LinkedElement?.FirstChildGuid))
            {
                SetFirstNode(graphNode, false);
            }

            AddElement(toAdd);
            SetNodeUnpassable(toAdd);

            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }

        public virtual void RemoveGraphNode(GraphNode graphNode)
        {
            if (graphNode is not TGraphNode toRemove) return;

            if (toRemove.guid.Equals(LinkedElement?.FirstChildGuid))
            {
                if (GraphNodes.Count > 1)
                {
                    ResetFirstNode(toRemove);
                }
                else
                {
                    ResetFirstNode();
                }
            }

            GraphNodes.Remove(toRemove.guid);
            toRemove.RemoveFromHierarchy();
            RemoveElement(toRemove);

            SaveScope.RequireSave();
            UpdateScope.RequireUpdate();
        }
        public virtual void RemoveGraphNode(string guid)
        {
            RemoveGraphNode(GetExistingGraphNode(guid));
        }
        public virtual void MoveGraphNode(string guid,Vector2 newPos,bool registerCommand = true)
        {
            var graphNode = GetExistingGraphNode(guid);
            if(graphNode != null)
            {
                graphNode.SetPosition(newPos, registerCommand);
            }
        }
        public virtual bool SelectGraphNode(string guid)
        {
            return SelectGraphNode(GetExistingGraphNode(guid));
        }
        public virtual bool SelectGraphNode(GraphNode graphNode)
        {
            if (graphNode == null) return false;
            AddToSelection(graphNode);
            return true;
        }
        public virtual bool SelectGraphEdge(string guid)
        {
            return SelectGraphEdge(getExistingGraphEdge(guid));
        }

        public bool SelectGraphEdge(IGraphEdge graphEdge)
        {
            if (graphEdge is not GraphEdge<TGraphNodeElement,TSwitcherElement> toSelect) return false;
            AddToSelection(toSelect);
            return true;
        }
        public virtual void SetFirstNode(string guid,bool registerCommand = true)
        {
            SetFirstNode(!String.IsNullOrEmpty(guid) ? GetExistingGraphNode(guid) : null, registerCommand);
        }
        public virtual void SetFirstNode(GraphNode graphNode, bool registerCommand = true)
        {
            if (registerCommand)
            {
                CommandRegistry.RegisterCommand(new SetFirstNodeCommand(LinkedElementGuid, Type, FirstNode == null ? null : FirstNode.guid, graphNode.guid));
            }
            FirstNode?.unmarkStartNode();
            FirstNode = (TGraphNode)graphNode;
            FirstNode?.markedAsStartNode();
        }
        public virtual TGraphNode GetExistingGraphNode(string guid)
        {
            if (guid != null && GraphNodes.TryGetValue(guid, out var node)) return node;
            return default;
        }
        public GraphNode GetExistingGraphNode(string guid,int inInterface)
        {
            return GetExistingGraphNode(guid);
        }
        public virtual GraphEdge<TGraphNodeElement,TSwitcherElement> getExistingGraphEdge(string guid)
        {
            if (guid != null && GraphEdges.TryGetValue(guid, out var edge) && edge is GraphEdge<TGraphNodeElement, TSwitcherElement> typedEdge) return typedEdge;
            return null;
        }
        protected virtual GraphViewChange OnGraphViewChanged(GraphViewChange change)
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
                            RemoveGraphEdgeByHand(graphEdge);
                        }
                    }
                    foreach (var element in change.elementsToRemove)
                    {
                        if (element is TGraphNode graphNode)
                        {
                            RemoveGraphNodeByHand(graphNode);
                        }
                    }
                }
            }
            return change;
        }
        public virtual void Update()
        {
            UpdateNodes();

            UpdateEdges();
        }
        protected virtual void UpdateNodes()
        {
            if (String.IsNullOrEmpty(LinkedElement?.FirstChildGuid))
            {
                ResetFirstNode();
            }

            foreach (var graphNode in GraphNodes.Values)
            {
                SetNodeUnpassable(graphNode);
                graphNode.Update();
            }
            SetNodePassable(FirstNode);
        }
        protected virtual void UpdateEdges()
        {
            var edgesToCheck = new List<IGraphEdge>(GraphEdges.Values);
            foreach (var graphEdge in edgesToCheck)
            {
                var inputGraphNode = GetExistingGraphNode(graphEdge.LinkedElement.inputElementGuid);
                var outputGraphNode = GetExistingGraphNode(graphEdge.LinkedElement.outputElementGuid);

                if (inputGraphNode == null || outputGraphNode == null)
                {
                    RemoveGraphEdgeByHand(graphEdge);
                    continue;
                }

                if (!inputGraphNode.isPassable || !outputGraphNode.isPassable) SetEdgeUnpassable((GraphEdge<TGraphNodeElement, TSwitcherElement>)graphEdge);
                else SetEdgePassable((GraphEdge<TGraphNodeElement, TSwitcherElement>)graphEdge);
            }
        }
        protected virtual void SetNodePassable(TGraphNode graphNode)
        {
            if (graphNode == null || graphNode.isPassable) return;
            graphNode.style.opacity = 1f;
            graphNode.isPassable = true;
            
            if (graphNode.linkedElement is not TGraphNodeElement element || element.SwitchersGuidList == null || element.SwitchersGuidList.Count == 0) return;
            
            foreach (var nodeSwitcherGuid in element.SwitchersGuidList)
            {
                var nodeSwitcher = FindElement(nodeSwitcherGuid) as NovaSwitcher;
                if (nodeSwitcher == null || String.IsNullOrEmpty(nodeSwitcher.inputElementGuid) || String.IsNullOrEmpty(nodeSwitcher.outputElementGuid)) continue;
                SetNodePassable(GetExistingGraphNode(nodeSwitcher.inputElementGuid));
            }
        }
        protected virtual void SetNodeUnpassable(TGraphNode graphNode)
        {
            if (graphNode == null || !graphNode.isPassable) return;
            graphNode.style.opacity = 0.5f;
            graphNode.isPassable = false;

            if (graphNode.linkedElement is not TGraphNodeElement element || element.SwitchersGuidList == null || element.SwitchersGuidList.Count == 0) return;

            foreach (var nodeSwitcherGuid in element.SwitchersGuidList)
            {
                var nodeSwitcher = FindElement(nodeSwitcherGuid) as NovaSwitcher;
                if (nodeSwitcher == null || String.IsNullOrEmpty(nodeSwitcher.inputElementGuid) || String.IsNullOrEmpty(nodeSwitcher.outputElementGuid)) continue;
                SetNodeUnpassable(GetExistingGraphNode(nodeSwitcher.inputElementGuid));
            }
        }
        protected virtual void SetEdgePassable(GraphEdge<TGraphNodeElement,TSwitcherElement> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 1f;
        }
        protected virtual void SetEdgeUnpassable(GraphEdge<TGraphNodeElement,TSwitcherElement> edge)
        {
            if (edge == null) return;
            edge.style.opacity = 0.5f;
        }

        public virtual void AddGraphNodeByHand(GraphNode graphNode,Vector2 pos)
        {
            //Register context to create data
            var newGraphNodeContext = SummonNewChildGraphViewNodeContext((TGraphNodeElement)graphNode.linkedElement, pos);
            RegisterContext(newGraphNodeContext);
                
            //Recording command
            var linkedData = newGraphNodeContext.LinkedData;
            if(linkedData != null) CommandRegistry.RegisterCommand(new AddNodeCommand(LinkedElementGuid, Type, linkedData.strongCopy() as IGraphViewNodeData));
            
            //In the end: Add node to graph view
            graphNode.linkedElement.SetParent(LinkedElement);
            AddGraphNode(graphNode);
        }
        public virtual void RemoveGraphNodeByHand(GraphNode graphNode)
        {
            //First: remove node from graph view
            graphNode.linkedElement.SetParent(null);
            RemoveGraphNode(graphNode);
                
            //Recording command
            var graphNodeContext = GetContext(graphNode.guid, graphNode.type);
            var linkedData = graphNodeContext?.LinkedData;
            if(linkedData != null) CommandRegistry.RegisterCommand(new RemoveNodeCommand(LinkedElementGuid, Type, linkedData.strongCopy() as IGraphViewNodeData));
                
            //Unregister context
            UnregisterContext(graphNodeContext);
        }

        public virtual void AddGraphEdgeByHand(IGraphEdge graphEdge)
        {
            var newEdgeContext = SummonNewChildEdgeContext(graphEdge.LinkedElement);
            RegisterContext(newEdgeContext);
            
            graphEdge.LinkedElement.SetParent(LinkedElement);
            
            //Put edge to graph view
            AddGraphEdge(graphEdge);
            
            //Recording command
            var linkedData = newEdgeContext.LinkedData;
            if(linkedData != null) CommandRegistry.RegisterCommand(new AddEdgeCommand(LinkedElementGuid, Type, linkedData.strongCopy() as IEdgeData));
        }
        public virtual void RemoveGraphEdgeByHand(IGraphEdge graphEdge)
        {
            graphEdge.LinkedElement.SetParent(null);
            
            //Remove edge from graph view
            RemoveGraphEdge(graphEdge);
            
            //Recording command
            var edgeContext = GetContext(graphEdge.Guid, NovaElementType.SWITCHER);
            var linkedData = edgeContext?.LinkedData;
            if(linkedData != null) CommandRegistry.RegisterCommand(new RemoveEdgeCommand(LinkedElementGuid, Type, linkedData.strongCopy() as IEdgeData));
            
            UnregisterContext(edgeContext);
        }
        
        public virtual void AddGraphNodeByCommand(IGraphViewNodeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.Guid) is not TGraphNodeElement addElement) return;
            RegisterContext(SummonNewChildGraphViewNodeContext(linkedData));
            
            addElement.SetParent(LinkedElement);
            
            var graphNode = SummonNewGraphNode(addElement, linkedData.pos);
            AddGraphNode(graphNode);
            
            //Not need to record command.
        }
        public virtual void RemoveGraphNodeByCommand(IGraphViewNodeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.Guid) is not TGraphNodeElement removeElement) return;
            
            RemoveGraphNode(linkedData.Guid);
            
            linkedData.linkedElement.SetParent(null);
            
            UnregisterContext(removeElement.Guid, removeElement.Type);
            
            //Not need to record command.
        }

        public virtual void AddGraphEdgeByCommand(IEdgeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.Guid) is not TSwitcherElement switcherElement) return;
            
            RegisterContext(SummonNewChildEdgeContext(linkedData));
            
            switcherElement.SetParent(LinkedElement);
            
            var graphEdge = SummonNewGraphEdge(switcherElement);
            AddGraphEdge(graphEdge);
        }

        public virtual void RemoveGraphEdgeByCommand(IEdgeData linkedData)
        {
            if (FindElement(linkedData.linkedElement.Guid) is not TSwitcherElement switcherElement) return;
            
            switcherElement.SetParent(null);
            
            RemoveGraphEdge(linkedData.Guid);
            
            UnregisterContext(linkedData.Guid, switcherElement.Type);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            evt.menu.AppendAction("Add Graph Node", _ =>
            {
                var e = SummonNewGraphNode(graphMousePosition);
                AddGraphNodeByHand(e,graphMousePosition);
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
        private void ResetFirstNode(TGraphNode excludeNode = null,bool registerCommand = false)
        {
            if (excludeNode == null)
            {
                SetFirstNode("",registerCommand);
                return;
            }
            foreach (var node in GraphNodes.Values)
            {
                if (node.guid == excludeNode.guid) continue;
                SetFirstNode(node, registerCommand);
                return;
            }
        } 
        private void CreateFloatingBackButton()
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
            
            _backButton.style.backgroundColor = new StyleColor(ThemedColor);
            _backButton.style.color = new StyleColor(Color.white);
            _backButton.style.fontSize = 13;
            _backButton.style.unityFontStyleAndWeight = FontStyle.Bold;

            _backButton.style.borderTopWidth = 1;
            _backButton.style.borderBottomWidth = 1;
            _backButton.style.borderLeftWidth = 1;
            _backButton.style.borderRightWidth = 1;
            
            _backButton.RegisterCallback<MouseEnterEvent>(_ => {
                _backButton.style.backgroundColor = new StyleColor(ThemedColor);
            });
            _backButton.RegisterCallback<MouseLeaveEvent>(_ => {
                _backButton.style.backgroundColor = new StyleColor(ThemedColor);
            });

            _backButton.style.display = DisplayStyle.None;
            Add(_backButton);
        }
        public void SetBackButtonVisible(bool isVisible)
        {
            _backButton.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    public interface INovaGraphView
    {
        string LinkedElementGuid { get; set; }
        INovaElement LinkedElement { get;}
        IEnumerable GraphNodes { get; }
        IEnumerable GraphEdges { get; }
        Vector2 MousePos { get;}
        System.Action OnRequestBackToParent { get; set; }
        GraphNode SummonNewGraphNode(NovaElement linkedElement, Vector2 pos);
        IGraphEdge SummonNewGraphEdge(NovaSwitcher linkedSwitcher);
        IGraphViewNodeContext SummonNewChildGraphViewNodeContext(NovaElement linkedElement,Vector2 pos);
        IGraphViewNodeContext SummonNewChildGraphViewNodeContext(IGraphViewNodeData linkedData);
        EdgeContext SummonNewChildEdgeContext(NovaSwitcher linkedSwitcher);
        EdgeContext SummonNewChildEdgeContext(IEdgeData linkedData);
        void AddGraphEdge(IGraphEdge graphEdge);
        void RemoveGraphEdge(IGraphEdge graphEdge);
        void RemoveGraphEdge(string guid);
        void AddGraphNode(GraphNode graphNode);
        void RemoveGraphNode(GraphNode graphNode);
        void RemoveGraphNode(string guid);
        GraphNode GetExistingGraphNode(string guid,int inInterface);
        void MoveGraphNode(string guid, Vector2 newPos, bool registerCommand = true);
        bool SelectGraphNode(string guid);
        bool SelectGraphNode(GraphNode graphNode);
        bool SelectGraphEdge(string guid);
        bool SelectGraphEdge(IGraphEdge graphEdge);
        void SetFirstNode(string guid, bool registerCommand = true);
        void SetFirstNode(GraphNode graphNode, bool registerCommand = true);
        void AddGraphNodeByHand(GraphNode graphNode, Vector2 pos);
        void RemoveGraphNodeByHand(GraphNode graphNode);
        void AddGraphEdgeByHand(IGraphEdge graphEdge);
        void RemoveGraphEdgeByHand(IGraphEdge graphEdge);
        void AddGraphNodeByCommand(IGraphViewNodeData linkedData);
        void RemoveGraphNodeByCommand(IGraphViewNodeData linkedData);
        void AddGraphEdgeByCommand(IEdgeData linkedData);
        void RemoveGraphEdgeByCommand(IEdgeData linkedData);
        void SetBackButtonVisible(bool isVisible);
        string GetActualName();
        void Update();
    }
}
