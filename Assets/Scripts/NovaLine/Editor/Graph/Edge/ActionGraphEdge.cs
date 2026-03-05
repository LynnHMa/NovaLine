
namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Action;
    using NovaLine.Switcher;

    public class ActionGraphEdge : GraphEdge<NovaAction, ActionSwitcher>
    {
        public override void generateNewLinkedElement()
        {
            linkedElement = new ActionSwitcher();
        }
    }
}
