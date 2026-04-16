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
using NovaLine.Script.Editor.Utils.Interface;
using NovaLine.Script.Editor.Utils.Scope;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Graph.Node
{
    public abstract class GraphNode : UnityEditor.Experimental.GraphView.Node,IGraphNode,IDoubleClick
    {
        private Vector2 _pos;
        private Vector2 _posWhenStartMoving;
        private bool _isMoving;
        private IVisualElementScheduledItem _moveSettleTimer;
        
        public virtual Color ThemedColor => Color.white;
        public virtual Vector2 Pos
        {
            get => _pos;
            set => _pos = value;
        }

        public virtual string LinkedElementGuid { get; set; }
        public virtual string Guid => LinkedElement?.Guid;
        public virtual NovaElementType Type => LinkedElement != null ? LinkedElement.Type : NovaElementType.NONE;
        public virtual NovaElement LinkedElement => FindElement(LinkedElementGuid);
        public override string title => LinkedElement?.GetActualName();

        public bool isPassable = true;

        string IGUID.Guid => Guid;

        protected GraphNode()
        {
            RemovePort();

            RegisterCallback<PointerDownEvent>(OnClick);

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

            var _titleButtonContainer = this.Q("title-button-container");
            if (_titleButtonContainer != null)
            {
                _titleButtonContainer.style.width = 0;
                _titleButtonContainer.style.minWidth = 0;
                _titleButtonContainer.style.overflow = Overflow.Hidden;
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

            var _inputContainer = this.Q("input");
            if (_inputContainer != null)
            {
                _inputContainer.style.flexGrow = 1;
                _inputContainer.style.justifyContent = Justify.Center;
                _inputContainer.style.paddingLeft = 8;
            }

            var _outputContainer = this.Q("output");
            if (_outputContainer != null)
            {
                _outputContainer.style.flexGrow = 1;
                _outputContainer.style.justifyContent = Justify.Center;
                _outputContainer.style.paddingRight = 8;
                _outputContainer.style.alignItems = Align.FlexEnd;
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
            LinkedElementGuid = linkedElement?.Guid;
            SetPosition(pos,false);
        }
        
        public virtual void SetPosition(Vector2 pos, bool registerCommand)
        {
            if (registerCommand)
            {
                CommandRegistry.RegisterCommand(BuildMoveCommand(this.Pos, pos));
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
                
                _moveSettleTimer ??= schedule.Execute(OnMoveSettled);
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
                    CommandRegistry.RegisterCommand(BuildMoveCommand(_posWhenStartMoving, _pos));
                }
            }
        }
        protected virtual MoveNodeCommand BuildMoveCommand(Vector2 oldPos, Vector2 newPos)
        {
            if (LinkedElement?.Parent == null) return null;
            return new MoveNodeCommand(LinkedElement.ParentGuid, LinkedElement.Parent.Type, new KeyValue<NovaElement, PosKeyValue>(LinkedElement, new PosKeyValue(oldPos, newPos)));
        }
        public override void OnSelected()
        {
            base.OnSelected();
            NovaWindow.SelectedGraphNode = this;

            if (LinkedElement == null) return;
            LinkedElement.ShowInInspector();
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            NovaWindow.SelectedGraphNode = null;

            var rootElement = (NovaElement)CurrentGraphViewNodeContext?.GraphView?.LinkedElement;

            if (rootElement == null) return;

            rootElement.ShowInInspector();
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
                if (currentGraphView != null && input != null && output != null)
                {
                    using (new CommandScope())
                    {
                        var inputConnections = input.connections.ToList();
                        for (var i = inputConnections.Count() - 1; i >= 0; i--)
                        {
                            if (inputConnections[i] is IGraphEdge ei)
                            {
                                currentGraphView.RemoveGraphEdgeByHand(ei);
                            }
                        }

                        var outputConnections = output.connections.ToList();
                        for (var i = outputConnections.Count() - 1; i >= 0; i--)
                        {
                            if (outputConnections[i] is IGraphEdge eo)
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
        public virtual void MarkedAsStartNode()
        {
            Label startBadge = new Label("Start")
            {
                name = "StartBadgeNode",
                style =
                {
                    backgroundColor = Color.white,
                    color = ThemedColor,
                    paddingLeft = 10,
                    paddingRight = 10,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    alignSelf = Align.Center,
                    fontSize = 14,
                    marginLeft = 10
                }
            };
            titleContainer.Insert(0, startBadge);
        }
        public virtual void UnmarkStartNode()
        {
            var badgeToRemove = titleContainer.Q<Label>("StartBadgeNode");

            if (badgeToRemove != null)
            {
                badgeToRemove.RemoveFromHierarchy();
            }
        }

        public virtual void Update()
        {
            title = LinkedElement?.GetActualName();
        }

        public void OnClick(PointerDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                OnDoubleClick(evt);
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }
        public virtual void OnDoubleClick(PointerDownEvent evt)
        {
        }
    }
    public interface IGraphNode : IGUID
    {
        void Update();
    }
}