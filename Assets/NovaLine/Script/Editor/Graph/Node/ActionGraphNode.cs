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
        public override Color ThemedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphNode(NovaElement linkedElement, Vector2 pos) : base(linkedElement, pos)
        {
            AddPort();
        }

        public override void AddPort()
        {
            if (LinkedElement is not NovaAction action) return;

            if (action.ActionType == ActionType.Meanwhile) return;

            var input = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, ThemedColor,"In");
            var output = GraphPort<NovaAction,ActionSwitcher>.Create<ActionGraphEdge>(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(float), action, ThemedColor,"Out");

            inputContainer.Add(input);
            outputContainer.Add(output);

            base.AddPort();
        }
        public override void Update()
        {
            base.Update();
            if (LinkedElement is NovaAction novaAction)
            {
                switch (novaAction.ActionType)
                {
                    case ActionType.Meanwhile when inputContainer.childCount > 0:
                        RemovePort();
                        break;
                    case ActionType.Sort when inputContainer.childCount == 0:
                        AddPort();
                        break;
                }
            }
        }
    }
}
