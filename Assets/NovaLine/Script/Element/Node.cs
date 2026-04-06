using System;
using System.Collections;
using NovaLine.Script.Action;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Node : NovaElement,IHasConditionElement
    {
        [SerializeField,HideInInspector] private string _conditionBeforeInvokeGuid;
        [SerializeField,HideInInspector] private string _conditionAfterInvokeGuid;
        
        public Condition conditionBeforeInvoke => FindElement(conditionBeforeInvokeGuid) as Condition;
        public Condition conditionAfterInvoke => FindElement(conditionAfterInvokeGuid) as Condition;

        public override NovaElementType type => NovaElementType.NODE;
        public string conditionBeforeInvokeGuid  { get => _conditionBeforeInvokeGuid;  set => _conditionBeforeInvokeGuid  = value; }
        public string conditionAfterInvokeGuid { get => _conditionAfterInvokeGuid; set => _conditionAfterInvokeGuid = value; }
        public Node()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            conditionAfterInvokeGuid = conditionAfter.guid;
            conditionBeforeInvokeGuid = conditionBefore.guid;
        }
        public Node(string name) : this()
        {
            this.name = name;
        }

        public IEnumerator run()
        {
            yield return conditionBeforeInvoke.waiting();
            foreach (var childGuid in childrenGuidList)
            {
                var child = FindElement(childGuid);
                if(child is not NovaAction action) continue;
                if (action.actionType == ActionType.Meanwhile)
                {
                    action.invoke().StartCoroutine();
                }
            }

            if (firstChild is NovaAction firstAction)
            {
                yield return firstAction.invoke();
            }
            else yield break;
            
            yield return conditionAfterInvoke.waiting();
            yield return null;
        }
        public override string getTypeName()
        {
            return "[Node]";
        }
        public override NovaElement copy()
        {
            var clone = base.copy();
            if (clone is not Node node) return clone;
            node.conditionBeforeInvokeGuid = conditionBeforeInvoke.copy().guid;
            node.conditionAfterInvokeGuid = conditionAfterInvoke.copy().guid;
            node.conditionBeforeInvoke.parentGuid = node.guid;
            node.conditionAfterInvoke.parentGuid = node.guid;
            return node;
        }
    }
}
