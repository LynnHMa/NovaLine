using UnityEngine;
using NovaLine.Editor.Graph.Port;
using NovaLine.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using NovaLine.Switcher;
using UnityEngine.UIElements;
using NovaLine.Editor.Utils;
using NovaLine.Editor.Window.Context;

namespace NovaLine.Editor.Graph.Node
{
    public class NodeGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.NODE_THEMED_COLOR;
        public NodeGraphNode(Element.Node node, Vector2 pos) : base(node, pos)
        {
            addPort();
        }
        public NodeGraphNode(string title, Vector2 pos) : this(new Element.Node(title), pos)
        {
            addPort();
        }
        public override string getType()
        {
            return "[Node]";
        }

        protected override void onDoubleClick(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                var nodeContext = (NodeContext)NovaWindow.GetContext(this,Window.Context.ContextType.NODE);

                if(nodeContext != null)
                {
                    NovaWindow.LoadContextInWindow(nodeContext);
                }
            }
        }
        public override void addPort()
        {
            if (linkedElement is not Element.Node node) return;

            var input = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(float), node, themedColor,"In");
            var output = GraphPort<Element.Node,NodeSwitcher>.Create<NodeGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(float), node, themedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.addPort();
        }
    }
}