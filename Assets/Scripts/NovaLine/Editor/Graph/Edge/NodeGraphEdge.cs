using UnityEngine;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
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
    }
}
