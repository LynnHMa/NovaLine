using System.Collections;
using System.Linq;
using NovaLine.Script.Element.Switcher;
using static NovaLine.Script.NovaElementRegistry;
using System;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script.Action
{
    [Serializable]
    public class NovaAction : NovaElement,INovaAction,IAroundConditionElement
    {
        [SerializeField,HideInInspector] private string _conditionBeforeInvokeGuid;
        [SerializeField,HideInInspector] private string _conditionAfterInvokeGuid;
        
        public ActionType ActionType = ActionType.Sort;
        
        public Condition ConditionBeforeInvoke => FindElement(ConditionBeforeInvokeGuid) as Condition;
        public Condition ConditionAfterInvoke => FindElement(ConditionAfterInvokeGuid) as Condition;
        public string ConditionBeforeInvokeGuid  { get => _conditionBeforeInvokeGuid;  set => _conditionBeforeInvokeGuid  = value; }
        public string ConditionAfterInvokeGuid { get => _conditionAfterInvokeGuid; set => _conditionAfterInvokeGuid = value; }

        public override NovaElementType Type => NovaElementType.ACTION;
        public NovaAction()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            ConditionAfterInvokeGuid = conditionAfter.Guid;
            ConditionBeforeInvokeGuid = conditionBefore.Guid;
        }
        public NovaAction(string name) : this()
        {
            this.name = name;
        }
        public virtual IEnumerator Invoke()
        {
            yield return ConditionBeforeInvoke.Waiting();

            yield return OnInvoke();

            yield return ConditionAfterInvoke.Waiting();

            yield return null;
            
            var firstSwitcherGuid = SwitchersGuidList.FirstOrDefault();
            if (FindElement(firstSwitcherGuid) is ActionSwitcher firstSwitcher)
            {
                yield return firstSwitcher.Next();
            }

            yield return null;
        }

        protected virtual IEnumerator OnInvoke()
        {
            yield return null;
        }
        public override string GetTypeName()
        {
            return "[Default Action]";
        }

        public override NovaElement Copy()
        {
            var clone = base.Copy();
            if (clone is not NovaAction action) return clone;
            action.ConditionBeforeInvokeGuid = ConditionBeforeInvoke.Copy().Guid;
            action.ConditionAfterInvokeGuid = ConditionAfterInvoke.Copy().Guid;
            action.ConditionBeforeInvoke.ParentGuid = action.Guid;
            action.ConditionAfterInvoke.ParentGuid = action.Guid;
            return action;
        }
    }
    public interface INovaAction
    {
        IEnumerator Invoke();
    }
}
public enum ActionType
{
    Meanwhile,
    Sort
}
