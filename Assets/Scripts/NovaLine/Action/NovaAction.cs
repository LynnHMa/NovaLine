namespace NovaLine.Action
{
    using System;
    using System.Threading.Tasks;
    using NovaLine.Element;
    using NovaLine.Interface;
    using NovaLine.Switcher;
    using UnityEngine;

    [Serializable]
    public class NovaAction : NovaElement
    {
        public static NovaAction EMPTY_ACTION = new();
        protected Action chainedAction;

        public Condition conditionBeforeInvoke;

        public Condition conditionAfterInvoke;

        public NovaAction()
        {
            guid = Guid.NewGuid().ToString();
        }

        public virtual string getType()
        {
            return "Default";
        }

        public virtual string getDescribation()
        {
            return "Nothing.";
        }

        public virtual async Task invoke()
        {
            await conditionBeforeInvoke.waiting();
            chainedAction?.Invoke();
            await conditionAfterInvoke.waiting();
        }

        public override void onGraphConnect(INovaSwitcher graphEdge)
        {
            if (graphEdge is ActionSwitcher actionSwitcher && parent is Node parentNode)
            {
                var outputAction = actionSwitcher.outputElement as NovaAction;
                var inputAction = actionSwitcher.inputElement as NovaAction;
                if (outputAction != null && inputAction != null)
                {
                    var oIndex = parentNode.actions.FindIndex(e => e.guid == outputAction.guid);
                    var iIndex = oIndex + 1;
                    if (iIndex >= 0)
                    {
                        parentNode?.actions?.Insert(iIndex, inputAction);
                    }
                }
            }
        }
    }
}
