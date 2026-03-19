using NovaLine.Action;
using NovaLine.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NovaLine.Editor.Utils;
using NovaLine.Editor.Graph.Port;
using NovaLine.Switcher;

namespace NovaLine.Editor.Graph.Node
{
    public class ActionGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphNode(NovaAction action, Vector2 pos) : base(action, pos)
        {
        }
        public override string getType()
        {
            return "[Action]";
        }
        public override void addPort()
        {
            if (linkedElement is not NovaAction action) return;

            if (action.type == ActionType.Meanwhile) return;

            var input = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, themedColor,"In");
            var output = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, themedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.addPort();
        }
        public override void update()
        {
            base.update();
            if (linkedElement is NovaAction novaAction)
            {
                if (novaAction.type == ActionType.Meanwhile && inputContainer.childCount > 0)
                {
                    removePort();
                }
                else if (novaAction.type == ActionType.Sort && inputContainer.childCount == 0)
                {
                    addPort();
                }
            }
        }
    }
}
