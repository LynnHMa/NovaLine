using System.Collections;
using System.Linq;
using NovaLine.Script.Element.Switcher;
using static NovaLine.Script.Registry.NovaElementRegistry;
using System;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script.Action
{
    /// <summary>
    /// Override to adding more custom actions :)
    /// </summary>
    [Serializable]
    public class NovaAction : NovaElement,INovaAction,IAroundConditionElement
    {
        [SerializeField,HideInInspector] private string _conditionBeforeInvokeGUID;
        [SerializeField,HideInInspector] private string _conditionAfterInvokeGUID;
        
        public ActionType ActionType = ActionType.Sort;
        
        public Condition ConditionBeforeInvoke => FindElement(ConditionBeforeInvokeGUID) as Condition;
        public Condition ConditionAfterInvoke => FindElement(ConditionAfterInvokeGUID) as Condition;
        public string ConditionBeforeInvokeGUID  { get => _conditionBeforeInvokeGUID;  set => _conditionBeforeInvokeGUID  = value; }
        public string ConditionAfterInvokeGUID { get => _conditionAfterInvokeGUID; set => _conditionAfterInvokeGUID = value; }
        public override Color ThemedColor => ColorExt.ACTION_THEMED_COLOR;
        public override NovaElementType Type => NovaElementType.Action;
        public NovaAction()
        {
            InitConditions();
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
            
            var firstSwitcherGUID = SwitchersGUIDList.FirstOrDefault();
            if (FindElement(firstSwitcherGUID) is ActionSwitcher firstSwitcher)
            {
                yield return firstSwitcher.Next();
            }
        }

        protected virtual IEnumerator OnInvoke()
        {
            yield break;
        }
        public override string GetTypeName()
        {
            return "[Default Action]";
        }

        public override NovaElement Copy()
        {
            var clone = base.Copy();
            if (clone is not NovaAction action) return clone;
            
            if (ConditionBeforeInvoke == null || ConditionAfterInvoke == null)
            {
                InitConditions();
            }
            if (ConditionBeforeInvoke != null && ConditionAfterInvoke != null)
            {
                action.ConditionBeforeInvokeGUID = ConditionBeforeInvoke.Copy().GUID;
                action.ConditionAfterInvokeGUID = ConditionAfterInvoke.Copy().GUID;
                action.ConditionBeforeInvoke.ParentGUID = action.GUID;
                action.ConditionAfterInvoke.ParentGUID = action.GUID;
            }
            
            return action;
        }
        
        public void InitConditions()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            ConditionAfterInvokeGUID = conditionAfter.GUID;
            ConditionBeforeInvokeGUID = conditionBefore.GUID;
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
