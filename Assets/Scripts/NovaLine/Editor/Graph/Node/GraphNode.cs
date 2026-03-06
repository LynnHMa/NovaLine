using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Element;
using NovaLine.Editor.Utils;
using UnityEditor.Experimental.GraphView;

namespace NovaLine.Editor.Graph.Node
{
    [Serializable]
    public class GraphNode : UnityEditor.Experimental.GraphView.Node
    {
        protected virtual Color themedColor => Color.white;

        public NovaElement targetObject { get; set; }

        public string guid { get; set; }

        public override string title
        {
            get
            {
                return base.title;
            }
            set
            {
                targetObject.name = value;
                base.title = targetObject.getActualName();
            }
        }

        public virtual Vector2 pos => new Vector2(base.GetPosition().x, base.GetPosition().y);

        private ObjectInspectorWrapper wrapper;

        public GraphNode()
        {
            guid = Guid.NewGuid().ToString();
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
            targetObject = element;
            title = element.name;
            guid = element.guid;
            addPort(element);
        }
        public void SetPosition(Vector2 pos)
        {
            base.SetPosition(new Rect(pos.x,pos.y,base.GetPosition().width,base.GetPosition().height));
        }
        public override void OnSelected()
        {
            base.OnSelected();
            NovaGraphWindow.nodeInInspector = this;

            if (targetObject == null) return;

            wrapper = ObjectInspectorWrapper.CreateInstance(targetObject);

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
        protected virtual void onDoubleClick(MouseDownEvent evt)
        {
        }
        public virtual string getType()
        {
            return "[Default]";
        }
        protected virtual void addPort(INovaElement element)
        {
        }
    }
}