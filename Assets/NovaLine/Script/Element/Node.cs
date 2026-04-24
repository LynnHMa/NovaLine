using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Element.Action;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface;
using UnityEngine;
using static NovaLine.Script.Registry.NovaElementRegistry;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Node : NovaElement,IAroundConditionElement
    {
        [SerializeField,HideInInspector] private string _conditionBeforeInvokeGUID;
        [SerializeField,HideInInspector] private string _conditionAfterInvokeGUID;
        
        public Condition ConditionBeforeInvoke => FindElement(ConditionBeforeInvokeGUID) as Condition;
        public Condition ConditionAfterInvoke => FindElement(ConditionAfterInvokeGUID) as Condition;
        public override Color ThemedColor => ColorExt.NODE_THEMED_COLOR;
        public override NovaElementType Type => NovaElementType.Node;
        public string ConditionBeforeInvokeGUID  { get => _conditionBeforeInvokeGUID;  set => _conditionBeforeInvokeGUID  = value; }
        public string ConditionAfterInvokeGUID { get => _conditionAfterInvokeGUID; set => _conditionAfterInvokeGUID = value; }
        public Node()
        {
            InitConditions();
        }
        public Node(string name) : this()
        {
            this.name = name;
        }

        public IEnumerator Run()
        {
            NovaPlayer.ResetScene(this);
            NovaPlayer.Instance.PlayingNodeGUID = GUID;
            yield return ConditionBeforeInvoke.Waiting();
            foreach (var childGUID in ChildrenGUIDList)
            {
                var child = FindElement(childGUID);
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
            foreach (var switcherConditionGUID in SwitchersGUIDList)
            {
                if (FindElement(switcherConditionGUID) is NodeSwitcher nodeSwitcher)
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
            
            if (ConditionBeforeInvoke == null || ConditionAfterInvoke == null)
            {
                InitConditions();
            }
            if (ConditionBeforeInvoke != null && ConditionAfterInvoke != null)
            {
                node.ConditionBeforeInvokeGUID = ConditionBeforeInvoke.Copy().GUID;
                node.ConditionAfterInvokeGUID = ConditionAfterInvoke.Copy().GUID;
                node.ConditionBeforeInvoke.ParentGUID = node.GUID;
                node.ConditionAfterInvoke.ParentGUID = node.GUID;
            }
            
            return node;
        }
        
        public bool ContainsDialogAction()
        {
            return ChildrenGUIDList.Find(GUID => FindElement(GUID) is DialogAction) != null;
        }
        
        public void InitConditions()
        {
            var conditionBefore = new Condition("Before Invoke",this);
            var conditionAfter = new Condition("After Invoke",this);
            ConditionAfterInvokeGUID = conditionAfter.GUID;
            ConditionBeforeInvokeGUID = conditionBefore.GUID;
        }
    }
}
