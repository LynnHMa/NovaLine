﻿using System;
 using System.Collections;
 using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class NodeSwitcher : NovaSwitcher
    {
        [SerializeField,HideInInspector] private string _switchConditionGuid;
        
        public Condition switchCondition => FindElement(switchConditionGuid) as Condition;
        public string switchConditionGuid{ get => _switchConditionGuid; set => _switchConditionGuid = value; }
        public NodeSwitcher(){ 
            var sc = new Condition("Switch Condition",this);
            switchConditionGuid = sc.Guid;
        }

        public override IEnumerator Next()
        {
            yield return switchCondition.Waiting();
            
            var nextNode = TryToFindInputElement();
            if (nextNode is Node node)
            {
                yield return node.Run();
            }
            
            yield return base.Next();
        }
        public override string GetTypeName()
        {
            return "[Node Edge]";
        }
        public override NovaElement Copy()
        {
            if (base.Copy() is not NodeSwitcher clone) return null;
            
            clone.switchConditionGuid = switchCondition.Copy().Guid;
            clone.switchCondition.ParentGuid = clone.Guid;
            
            return clone;
        }
    }
}
