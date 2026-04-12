using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Script.Element;
using System.Linq;
using NovaLine.Script.Editor.Graph.Edge;
using NovaLine.Script.Utils.Interface;
using NovaLine.Script.Editor.Window;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Graph.Node
{
    public abstract class GraphNode : UnityEditor.Experimental.GraphView.Node,IGraphNode
    {
        private Vector2 _pos;
        private Vector2 _posWhenStartMoving;
        private bool _isMoving = false;
        private IVisualElementScheduledItem _moveSettleTimer;
        
        protected virtual Color ThemedColor => Color.white;
        public virtual Vector2 pos
        {
            get => _pos;
            set => _pos = value;
        }

        public virtual string linkedElementGuid { get; set; }
        public virtual string guid => linkedElement?.Guid;
        public virtual NovaElementType type => linkedElement != null ? linkedElement.Type : NovaElementType.NONE;
        public virtual NovaElement linkedElement => FindElement(linkedElementGuid);
        public override string title => linkedElement?.GetActualName();

        public bool isPassable = true;

        string IGUID.Guid => guid;

        protected GraphNode()
        {
            RemovePort();

            RegisterCallback<MouseDownEvent>(OnDoubleClick);

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

            Color.RGBToHSV(ThemedColor, out float h, out float s, out float v);

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
        }
        protected GraphNode(NovaElement linkedElement, Vector2 pos) : this()
        {
            linkedElementGuid = linkedElement?.Guid;
            SetPosition(pos,false);
        }
        
        public virtual void SetPosition(Vector2 pos, bool registerCommand)
        {
            if (registerCommand)
            {
                CommandRegistry.RegisterCommand(buildMoveCommand(this.pos, pos));
            }

            _pos = pos;
            _isMoving = false; 
            
            SetPosition(new Rect(pos.x, pos.y, base.GetPosition().width, base.GetPosition().height));
        }
        
        public override void SetPosition(Rect newPos)
        {
            Vector2 targetPos = new Vector2(newPos.x, newPos.y);
            
            if (_pos != targetPos)
            {
                if (!_isMoving)
                {
                    _posWhenStartMoving = _pos;
                    _isMoving = true;
                }

                _pos = targetPos; 
                
                if (_moveSettleTimer == null)
                {
                    _moveSettleTimer = schedule.Execute(OnMoveSettled);
                }
                _moveSettleTimer.ExecuteLater(100); 
            }

            base.SetPosition(newPos); 
        }
        
        private void OnMoveSettled()
        {
            if (_isMoving)
            {
                _isMoving = false;
                
                if (_posWhenStartMoving != _pos)
                {
                    CommandRegistry.RegisterCommand(buildMoveCommand(_posWhenStartMoving, _pos));
                }
            }
        }
        protected virtual MoveNodeCommand buildMoveCommand(Vector2 oldPos, Vector2 newPos)
        {
            if (linkedElement?.Parent == null) return null;
            return new MoveNodeCommand(linkedElement.ParentGuid, linkedElement.Parent.Type, new KeyValue<NovaElement, PosKeyValue>(linkedElement, new PosKeyValue(oldPos, newPos)));
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

            var rootElement = (NovaElement)CurrentGraphViewNodeContext?.GraphView?.LinkedElement;

            if (rootElement == null) return;

            rootElement.ShowInInspector();
        }
        public virtual string GetType()
        {
            return "[Default]";
        }
        public virtual void AddPort()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
        public virtual void RemovePort()
        {
            if (inputContainer.childCount > 0 && outputContainer.childCount > 0)
            {
                var input = inputContainer[0] as UnityEditor.Experimental.GraphView.Port;
                var output = outputContainer[0] as UnityEditor.Experimental.GraphView.Port;
                var currentGraphView = CurrentGraphViewNodeContext?.GraphView;
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
                                currentGraphView.RemoveGraphEdgeByHand(ei);
                            }
                        }

                        var outputConnections = output.connections.ToList();
                        for (var i = outputConnections.Count() - 1; i >= 0; i--)
                        {
                            var eo = outputConnections[i] as IGraphEdge;
                            if (eo != null)
                            {
                                currentGraphView.RemoveGraphEdgeByHand(eo);
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
            startBadge.style.color = ThemedColor;
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

        public virtual void Update()
        {
            title = linkedElement?.GetActualName();
        }

        protected virtual void OnDoubleClick(MouseDownEvent evt)
        {
        }
    }
    public interface IGraphNode : IGUID
    {
        void Update();
    }
}