
namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using NovaLine.Action;
    using NovaLine.Switcher;
    using UnityEngine;

    public class ActionGraphEdge : GraphEdge<NovaAction, ActionSwitcher>
    {
        protected override Color themedColor => ColorExt.orange;
        public override ActionSwitcher generateNewLinkedElement()
        {
            linkedElement = new ActionSwitcher();
            return linkedElement;
        }
    }
}
