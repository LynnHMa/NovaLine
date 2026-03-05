using NovaLine.Utils;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Element;
    using NovaLine.Switcher;

    public class NodeGraphEdge : GraphEdge<Node,NodeSwitcher>
    {
        public NodeSwitcher nodeSwitcher { get; set;}

        private ObjectInspectorWrapper wrapper;

        public override void generateNewLinkedElement()
        {
            linkedElement = new NodeSwitcher();
        }
        public override void OnSelected()
        {
            base.OnSelected();

            if (wrapper == null)
            {
                wrapper = ScriptableObject.CreateInstance<ObjectInspectorWrapper>();

                wrapper.hideFlags = HideFlags.DontSave;

                wrapper.name = "Next Node";
            }
            var parentSwitcher = linkedElement;
            nodeSwitcher = parentSwitcher == null ? new NodeSwitcher() : parentSwitcher;
            guid = nodeSwitcher.guid;
            wrapper.objectData = nodeSwitcher;

            Selection.activeObject = wrapper;
        }
    }
}
