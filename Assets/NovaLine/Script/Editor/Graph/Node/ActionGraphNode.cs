using NovaLine.Script.Action;
using NovaLine.Script.Editor.Graph.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Graph.Port;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.Node
{
    public class ActionGraphNode : GraphNode
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphNode(NovaElement linkedElement, Vector2 pos) : base(linkedElement, pos)
        {
            addPort();
        }
        public override string getType()
        {
            return "[Action]";
        }
        public override void addPort()
        {
            if (linkedElement is not NovaAction action) return;

            if (action.actionType == ActionType.Meanwhile) return;

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
                if (novaAction.actionType == ActionType.Meanwhile && inputContainer.childCount > 0)
                {
                    removePort();
                }
                else if (novaAction.actionType == ActionType.Sort && inputContainer.childCount == 0)
                {
                    addPort();
                }
            }
        }
    }
}
