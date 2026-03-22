using NovaLine.Action;
using NovaLine.Switcher;
using System;
using NovaLine.Element.Switcher;
using UnityEngine;

namespace NovaLine.Data.Edge
{
    [Serializable]
    public class ActionEdgeData : EdgeData<NovaAction, ActionSwitcher>
    {
        [SerializeReference] private ActionSwitcher _linkedSwitcher;

        public override ActionSwitcher linkedSwitcher
        {
            get => _linkedSwitcher;
            set => _linkedSwitcher = value;
        }

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
