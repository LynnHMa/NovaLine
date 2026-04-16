using NovaLine.Script.Editor.Window;
using NovaLine.Script.Element.Switcher;
using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Script.Editor.Utils;

namespace NovaLine.Script.Editor.Graph.Edge
{


    public class NodeGraphEdge : GraphEdge<Element.Node,NodeSwitcher>
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

        public override void OnDoubleClick(PointerDownEvent evt)
        {
            var switchCondition = LinkedElement.switchCondition;
            if (switchCondition != null)
            {
                NovaWindow.LoadConditionContextDirect(switchCondition, "Switch Condition");
            }
        }
    }
}
