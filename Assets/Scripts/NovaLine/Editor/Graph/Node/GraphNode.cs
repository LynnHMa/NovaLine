using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Element;
using NovaLine.Editor.Utils;

namespace NovaLine.Editor.Graph.Node
{
    [Serializable]
    public class GraphNode : UnityEditor.Experimental.GraphView.Node
    {
        protected virtual Color themedColor => Color.white;

        public virtual string guid => linkedElement.guid;

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

        public override string title => linkedElement?.getActualName();

        public virtual Vector2 pos => new Vector2(base.GetPosition().x, base.GetPosition().y);

        private ObjectInspectorWrapper wrapper;

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
        }
        public GraphNode(NovaElement element, Vector2 pos) : this()
        {
            SetPosition(new Rect(pos.x, pos.y, 200, 150));
            linkedElement = element;
        }
        public void SetPosition(Vector2 pos)
        {
            base.SetPosition(new Rect(pos.x,pos.y,base.GetPosition().width,base.GetPosition().height));
        }
        public override void OnSelected()
        {
            base.OnSelected();
            NovaGraphWindow.nodeInInspector = this;

            if (linkedElement == null) return;

            wrapper = ObjectInspectorWrapper.CreateInstance(linkedElement);

            wrapper.hideFlags = HideFlags.DontSave;

            wrapper.name = getType();

            Selection.activeObject = wrapper;
        }
        public override void OnUnselected()
        {
            base.OnUnselected();
            NovaGraphWindow.nodeInInspector = null;

            var activeRoot = NovaGraphWindow.getMainWindowInstance()?.currentOpenedGraphView?.graphView?.root;

            if (activeRoot == null) return;

            wrapper = ObjectInspectorWrapper.CreateInstance(activeRoot);

            wrapper.hideFlags = HideFlags.DontSave;

            wrapper.name = getType();

            Selection.activeObject = wrapper;
        }
        public virtual string getType()
        {
            return "[Default]";
        }
        public virtual void addPort()
        {
        }
        public virtual void removePort()
        {
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
        protected virtual void onDoubleClick(MouseDownEvent evt)
        {
        }
    }
}