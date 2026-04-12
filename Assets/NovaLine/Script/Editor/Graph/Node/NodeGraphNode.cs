using UnityEngine;
using NovaLine.Script.Editor.Graph.Port;
using NovaLine.Script.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Context;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.Node
{
    public class NodeGraphNode : GraphNode
    {
        protected override Color ThemedColor => ColorExt.NODE_THEMED_COLOR;
        public NodeGraphNode(NovaElement linkedElement, Vector2 pos) : base(linkedElement, pos)
        {
            AddPort();
        }
        public override string GetType()
        {
            return "[Node]";
        }

        protected override void OnDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                var nodeContext = (NodeNodeContext)GetContext(this,NovaElementType.NODE);

                if(nodeContext != null)
                {
                    NovaWindow.LoadContextInWindow(nodeContext);
                }
            }
        }
        public override void AddPort()
        {
            if (linkedElement is not Element.Node node) return;

            var input = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(float), node, ThemedColor,"In");
            var output = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(float), node, ThemedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.AddPort();
        }
    }
}