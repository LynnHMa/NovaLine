using NovaLine.Script.Element.Switcher;
using UnityEngine;

namespace NovaLine.Script.Editor.Graph.Edge
{
    using NovaLine.Script.Editor.Utils;
    using NovaLine.Script.Element;

    public class NodeGraphEdge : GraphEdge<Node,NodeSwitcher>
    {
        protected override Color ThemedColor => ColorExt.NODE_THEMED_COLOR;
        public NodeGraphEdge()
        {
        }
        public override NodeSwitcher GenerateNewLinkedElement()
        {
            LinkedElement = new NodeSwitcher();
            return LinkedElement;
        }
    }
}
