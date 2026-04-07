using System;
using NovaLine.Script.Element.Switcher;
using UnityEngine;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class ActionEdgeData : EdgeData<ActionSwitcher>
    {
        public override string name => "Next Action";
        public override string describtion => "The next action.";

        public ActionEdgeData()
        {
        }
        public ActionEdgeData(ActionSwitcher switcher) : base(switcher)
        {
        }
    }
}
