using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Action;
using NovaLine.Script.Element.Action;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Node : NovaElement,IAroundConditionElement
    {
        [SerializeField,HideInInspector] private string _conditionBeforeInvokeGuid;
        [SerializeField,HideInInspector] private string _conditionAfterInvokeGuid;
        
        public Condition ConditionBeforeInvoke => FindElement(ConditionBeforeInvokeGuid) as Condition;
        public Condition ConditionAfterInvoke => FindElement(ConditionAfterInvokeGuid) as Condition;

        public override NovaElementType Type => NovaElementType.NODE;
        public string ConditionBeforeInvokeGuid  { get => _conditionBeforeInvokeGuid;  set => _conditionBeforeInvokeGuid  = value; }
        public string ConditionAfterInvokeGuid { get => _conditionAfterInvokeGuid; set => _conditionAfterInvokeGuid = value; }
        public Node()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            ConditionAfterInvokeGuid = conditionAfter.Guid;
            ConditionBeforeInvokeGuid = conditionBefore.Guid;
        }
        public Node(string name) : this()
        {
            this.name = name;
        }

        public IEnumerator Run()
        {
            NovaPlayer.ResetScene(this);
            yield return ConditionBeforeInvoke.Waiting();
            foreach (var childGuid in ChildrenGuidList)
            {
                var child = FindElement(childGuid);
                if(child is not NovaAction action) continue;
                if (action.ActionType == ActionType.Meanwhile)
                {
                    action.Invoke().StartCoroutine();
                }
            }

            if (FirstChild is NovaAction firstAction)
            {
                yield return firstAction.Invoke();
            }
            
            yield return ConditionAfterInvoke.Waiting();

            yield return null;

            List<IEnumerator> switcherConditions = new();
            foreach (var switcherConditionGuid in SwitchersGuidList)
            {
                if (FindElement(switcherConditionGuid) is NodeSwitcher nodeSwitcher)
                {
                    switcherConditions.Add(nodeSwitcher.Next());
                }
            }
            yield return switcherConditions.WhenAny();
            
            yield return null;
        }
        public override string GetTypeName()
        {
            return "[Node]";
        }
        public override NovaElement Copy()
        {
            var clone = base.Copy();
            if (clone is not Node node) return clone;
            node.ConditionBeforeInvokeGuid = ConditionBeforeInvoke.Copy().Guid;
            node.ConditionAfterInvokeGuid = ConditionAfterInvoke.Copy().Guid;
            node.ConditionBeforeInvoke.ParentGuid = node.Guid;
            node.ConditionAfterInvoke.ParentGuid = node.Guid;
            return node;
        }

        public bool ContainsDialogAction()
        {
            return ChildrenGuidList.Find(guid => FindElement(guid) is DialogAction) != null;
        }
        public bool ContainsEntityAction()
        {
            return ChildrenGuidList.Find(guid => FindElement(guid) is EntityAction) != null;
        }
    }
}
