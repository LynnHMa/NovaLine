using UnityEditor;
using UnityEngine;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using NovaLine.Editor.Window;
    using NovaLine.Element;
    using NovaLine.Switcher;

    public class NodeGraphEdge : GraphEdge<Node,NodeSwitcher>
    {
        protected override Color themedColor => ColorExt.NODE_THEMED_COLOR;
        public NodeGraphEdge() : base()
        {
        }
        public override NodeSwitcher generateNewLinkedElement()
        {
            linkedElement = new NodeSwitcher();
            return linkedElement;
        }
        public override void OnSelected()
        {
            base.OnSelected();

            NovaWindow.SelectedGraphEdge = this;

            if (linkedElement == null) return;

            linkedElement.ShowInInspector();
        }
        public override void OnUnselected()
        {
            base.OnUnselected();

            NovaWindow.SelectedGraphEdge = null;

            var activeRoot = (NovaElement)NovaWindow.GetMainWindowInstance()?.currentGraphViewContext?.graphView?.linkedElement;

            if (activeRoot == null) return;

            activeRoot.ShowInInspector();
        }
    }
}
