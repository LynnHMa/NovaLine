
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Editor.Graph.Edge
{
    using NovaLine.Script.Editor.Utils;
    using NovaLine.Script.Action;
    using UnityEngine;

    public class ActionGraphEdge : GraphEdge<NovaAction, ActionSwitcher>
    {
        protected override Color themedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphEdge()
        {
        }
        public override ActionSwitcher generateNewLinkedElement()
        {
            linkedElement = new ActionSwitcher();
            return linkedElement;
        }
    }
}
