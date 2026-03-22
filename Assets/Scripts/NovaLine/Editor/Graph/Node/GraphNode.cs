using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Element;
using System.Linq;
using NovaLine.Editor.Graph.Edge;
using NovaLine.Utils.Interface;
using NovaLine.Editor.Window;
using static NovaLine.Editor.Window.WindowContextRegistry;
using NovaLine.Editor.Window.Command;
using UnityEditor.Experimental.GraphView;
using Editor.Utils.Ext;

namespace NovaLine.Editor.Graph.Node
{
    public abstract class GraphNode : UnityEditor.Experimental.GraphView.Node,IGraphNode
    {
        protected virtual Color themedColor => Color.white;
        public virtual Vector2 pos => new Vector2(base.GetPosition().x, base.GetPosition().y);
        public virtual string guid => linkedElement?.guid;
        public virtual NovaElementType type => linkedElement != null ? linkedElement.type : NovaElementType.NONE;
        public override string title => linkedElement?.getActualName();
        public GraphView graphView => GetFirstAncestorOfType<GraphView>();

        private NovaElement _linkedElement;
        public virtual NovaElement linkedElement
        {
            get => _linkedElement;
            set
            {
                _linkedElement = value;
                base.title = value?.getActualName();
            }
        }
        public bool isPassable = true;
        public bool isDragging = false;
        private Vector2 posWhenStartDragging;

        string IGUID.guid { get => guid; set { return; } }

        public GraphNode()
        {
            removePort();

            RegisterCallback<MouseDownEvent>(onDoubleClick);

            var titleLabel = this.Q<Label>("title-label");
            if (titleLabel != null)
            {
                titleLabel.style.position = Position.Relative; 
                titleLabel.style.flexGrow = 1;
                titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                titleLabel.style.color = Color.white;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 14;
            }

            style.minWidth = 190;

            var titleButtonContainer = this.Q("title-button-container");
            if (titleButtonContainer != null)
            {
                titleButtonContainer.style.width = 0;
                titleButtonContainer.style.minWidth = 0;
                titleButtonContainer.style.overflow = Overflow.Hidden;
            }

            var collapseButton = this.Q("collapse-button");
            if (collapseButton != null)
                collapseButton.style.display = DisplayStyle.None;

            Color.RGBToHSV(themedColor, out float h, out float s, out float v);

            Color titleColor = Color.HSVToRGB(h, s * 0.85f, v * 0.75f);
            Color dividerColor = Color.HSVToRGB(h, s * 0.70f, v * 1.00f);
            Color contentsColor = Color.HSVToRGB(h, s * 0.60f, v * 0.25f);

            var titleBar = this.Q("title");
            if (titleBar != null)
            {
                titleBar.style.justifyContent = Justify.Center;
                titleBar.style.backgroundColor = titleColor;
                titleBar.style.borderBottomWidth = 1;
                titleBar.style.borderBottomColor = dividerColor;
            }

            var contents = this.Q("contents");
            if (contents != null)
                contents.style.backgroundColor = contentsColor;

            var inputContainer = this.Q("input");
            if (inputContainer != null)
            {
                inputContainer.style.flexGrow = 1;
                inputContainer.style.justifyContent = Justify.Center;
                inputContainer.style.paddingLeft = 8;
            }

            var outputContainer = this.Q("output");
            if (outputContainer != null)
            {
                outputContainer.style.flexGrow = 1;
                outputContainer.style.justifyContent = Justify.Center;
                outputContainer.style.paddingRight = 8;
                outputContainer.style.alignItems = Align.FlexEnd;
            }

            var top = this.Q("top");
            if (top != null)
            {
                top.style.flexDirection = FlexDirection.Row;
                top.style.justifyContent = Justify.SpaceBetween;
            }

            RegisterCallback<PointerDownEvent>(onMouseDown, TrickleDown.TrickleDown);
        }
        public GraphNode(NovaElement element, Vector2 pos) : this()
        {
            linkedElement = element;
            SetPosition(pos,false);
        }
        public virtual void SetPosition(Vector2 pos,bool registerCommand)
        {
            if (registerCommand) CommandRegistry.Register(buildMoveCommand(this.pos, pos));
            base.SetPosition(new Rect(pos.x, pos.y, base.GetPosition().width, base.GetPosition().height));
        }
        private void onMouseDown(PointerDownEvent evt)
        {
            if(evt.button == 0 && !isDragging)
            {
                posWhenStartDragging = pos;
                isDragging = true;

                if (graphView != null)
                {
                    graphView.RegisterCallback<PointerUpEvent>(onMouseUp, TrickleDown.TrickleDown);
                }
            }
        }
        private void onMouseUp(PointerUpEvent evt)
        {
            if (evt.button == 0 && isDragging)
            {
                isDragging = false;

                if (graphView != null)
                {
                    graphView.UnregisterCallback<PointerUpEvent>(onMouseUp, TrickleDown.TrickleDown);
                }

                if (posWhenStartDragging != pos)
                {
                    CommandRegistry.Register(buildMoveCommand(posWhenStartDragging, pos));
                }
                posWhenStartDragging = pos;
            }
        }
        protected virtual MoveNodeCommand buildMoveCommand(Vector2 oldPos, Vector2 newPos)
        {
            if (linkedElement == null || linkedElement.parent == null) return null;
            return new MoveNodeCommand(linkedElement.parent.guid, linkedElement.parent.type, new KeyValue<NovaElement, PosKeyValue>(linkedElement, new PosKeyValue(oldPos, newPos)));
        }
        public override void OnSelected()
        {
            base.OnSelected();
            NovaWindow.SelectedGraphNode = this;

            if (linkedElement == null) return;

            linkedElement.ShowInInspector();
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            NovaWindow.SelectedGraphNode = null;

            var rootElement = (NovaElement)CurrentGraphViewContext?.graphView?.linkedElement;

            if (rootElement == null) return;

            rootElement.ShowInInspector();
        }
        public virtual string getType()
        {
            return "[Default]";
        }
        public virtual void addPort()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual void removePort()
        {
            if (inputContainer.childCount > 0 && outputContainer.childCount > 0)
            {
                var input = inputContainer[0] as UnityEditor.Experimental.GraphView.Port;
                var output = outputContainer[0] as UnityEditor.Experimental.GraphView.Port;
                var currentGraphView = CurrentGraphViewContext?.graphView;
                if (currentGraphView != null)
                {
                    using (new CommandScope())
                    {
                        var inputConnections = input.connections.ToList();
                        for (var i = inputConnections.Count() - 1; i >= 0; i--)
                        {
                            var ei = inputConnections[i] as IGraphEdge;
                            if (ei != null)
                            {
                                currentGraphView.removeGraphEdge(ei,false);
                            }
                        }

                        var outputConnections = output.connections.ToList();
                        for (var i = outputConnections.Count() - 1; i >= 0; i--)
                        {
                            var eo = outputConnections[i] as IGraphEdge;
                            if (eo != null)
                            {
                                currentGraphView.removeGraphEdge(eo,false);
                            }
                        }
                    }
                }
            }

            inputContainer.Clear();
            outputContainer.Clear();
            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual void markedAsStartNode()
        {
            Label startBadge = new Label("Start");
            startBadge.name = "StartBadgeNode";
            startBadge.style.backgroundColor = Color.white;
            startBadge.style.color = themedColor;
            startBadge.style.paddingLeft = 10;
            startBadge.style.paddingRight = 10;
            startBadge.style.borderBottomLeftRadius = 5;
            startBadge.style.borderBottomRightRadius = 5;
            startBadge.style.borderTopLeftRadius = 5;
            startBadge.style.borderTopRightRadius = 5;
            startBadge.style.alignSelf = Align.Center;
            startBadge.style.fontSize = 14;
            startBadge.style.marginLeft = 10;
            titleContainer.Insert(0, startBadge);
        }
        public virtual void unmarkStartNode()
        {
            var badgeToRemove = titleContainer.Q<Label>("StartBadgeNode");

            if (badgeToRemove != null)
            {
                badgeToRemove.RemoveFromHierarchy();
            }
        }

        public virtual void update()
        {
            title = linkedElement?.getActualName();
        }

        protected virtual void onDoubleClick(MouseDownEvent evt)
        {
        }
    }
    public interface IGraphNode : IGUID
    {
        void update();
    }
}