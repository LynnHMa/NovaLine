using UnityEditor;
using UnityEngine;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using NovaLine.Element;
    using NovaLine.Switcher;

    public class NodeGraphEdge : GraphEdge<Node,NodeSwitcher>
    {
        protected override Color themedColor => ColorExt.red;
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

            wrapper = ObjectInspectorWrapper.CreateInstance(linkedElement);

            wrapper.hideFlags = HideFlags.DontSave;

            wrapper.name = "Next Node";
            var parentSwitcher = linkedElement;
            linkedElement = parentSwitcher == null ? generateNewLinkedElement() : parentSwitcher;

            var activeRoot = NovaWindow.GetMainWindowInstance()?.currentGraphViewContext?.graphView?.root;
            if (activeRoot != null && activeRoot is NovaElement currentElement)
            {
                var p = currentElement.parent;
                while (p != null)
                {
                    wrapper.parentNodes.Add(p);
                    p = p.parent;
                }
                wrapper.parentNodes.Reverse();
            }
            wrapper.parentNodes.Add(activeRoot);

            Selection.activeObject = wrapper;
        }
        public override void OnUnselected()
        {
            base.OnUnselected();

            NovaWindow.SelectedGraphEdge = null;

            var activeRoot = NovaWindow.GetMainWindowInstance()?.currentGraphViewContext?.graphView?.root;

            if (activeRoot == null) return;

            wrapper = ObjectInspectorWrapper.CreateInstance(activeRoot);

            wrapper.hideFlags = HideFlags.DontSave;

            Selection.activeObject = wrapper;
        }
    }
}
