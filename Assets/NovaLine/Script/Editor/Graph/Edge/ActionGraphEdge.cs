
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Ext;

namespace NovaLine.Script.Editor.Graph.Edge
{
    using NovaLine.Script.Editor.Utils;
    using NovaLine.Script.Action;
    using UnityEngine;

    public class ActionGraphEdge : GraphEdge<NovaAction, ActionSwitcher>
    {
        protected override Color ThemedColor => ColorExt.ACTION_THEMED_COLOR;
        public ActionGraphEdge()
        {
        }
        public override ActionSwitcher GenerateNewLinkedElement()
        {
            LinkedElement = new ActionSwitcher();
            return LinkedElement;
        }
    }
}
