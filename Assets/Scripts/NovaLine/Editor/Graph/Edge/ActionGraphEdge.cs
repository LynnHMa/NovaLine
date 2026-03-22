
using NovaLine.Element.Switcher;

namespace NovaLine.Editor.Graph.Edge
{
    using NovaLine.Editor.Utils;
    using NovaLine.Action;
    using NovaLine.Switcher;
    using UnityEngine;

    public class ActionGraphEdge : GraphEdge<NovaAction, ActionSwitcher>
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphEdge() : base()
        {
        }
        public override ActionSwitcher generateNewLinkedElement()
        {
            linkedElement = new ActionSwitcher();
            return linkedElement;
        }
    }
}
