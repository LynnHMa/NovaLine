﻿using System;
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
            switchConditionGuid = sc.guid;
        }
        public override string getTypeName()
        {
            return "[Node Edge]";
        }
        public override NovaElement copy()
        {
            if (base.copy() is not NodeSwitcher clone) return null;
            
            clone.switchConditionGuid = switchCondition.copy().guid;
            clone.switchCondition.parentGuid = clone.guid;
            
            return clone;
        }
    }
}
