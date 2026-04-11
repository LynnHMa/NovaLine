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
        
        public ActionType actionType = ActionType.Sort;
        
        public Condition conditionBeforeInvoke => FindElement(conditionBeforeInvokeGuid) as Condition;
        public Condition conditionAfterInvoke => FindElement(conditionAfterInvokeGuid) as Condition;
        public string conditionBeforeInvokeGuid  { get => _conditionBeforeInvokeGuid;  set => _conditionBeforeInvokeGuid  = value; }
        public string conditionAfterInvokeGuid { get => _conditionAfterInvokeGuid; set => _conditionAfterInvokeGuid = value; }

        public override NovaElementType type => NovaElementType.ACTION;
        public NovaAction()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            conditionAfterInvokeGuid = conditionAfter.guid;
            conditionBeforeInvokeGuid = conditionBefore.guid;
        }
        public NovaAction(string name) : this()
        {
            this.name = name;
        }
        public virtual IEnumerator invoke()
        {
            yield return conditionBeforeInvoke.waiting();

            yield return onInvoke();

            yield return conditionAfterInvoke.waiting();

            yield return null;
            
            var firstSwitcherGuid = switchersGuidList.FirstOrDefault();
            if (FindElement(firstSwitcherGuid) is ActionSwitcher firstSwitcher)
            {
                yield return firstSwitcher.next();
            }

            yield return null;
        }

        protected virtual IEnumerator onInvoke()
        {
            yield return null;
        }
        public override string getTypeName()
        {
            return "[Default Action]";
        }

        public override NovaElement copy()
        {
            var clone = base.copy();
            if (clone is not NovaAction action) return clone;
            action.conditionBeforeInvokeGuid = conditionBeforeInvoke.copy().guid;
            action.conditionAfterInvokeGuid = conditionAfterInvoke.copy().guid;
            action.conditionBeforeInvoke.parentGuid = action.guid;
            action.conditionAfterInvoke.parentGuid = action.guid;
            return action;
        }
    }
    public interface INovaAction
    {
        IEnumerator invoke();
    }
}
public enum ActionType
{
    Meanwhile,
    Sort
}
