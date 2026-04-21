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
        
        public Condition SwitchCondition => FindElement(SwitchConditionGuid) as Condition;
        public string SwitchConditionGuid{ get => _switchConditionGuid; set => _switchConditionGuid = value; }
        public NodeSwitcher()
        {
            InitCondition();
        }

        public override IEnumerator Next()
        {
            yield return SwitchCondition.Waiting();
            
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

            if (SwitchCondition == null)
            {
                InitCondition();
            }
            if (SwitchCondition != null)
            {
                clone.SwitchConditionGuid = SwitchCondition.Copy().Guid;
                clone.SwitchCondition.ParentGuid = clone.Guid;
            }
            
            return clone;
        }

        private void InitCondition()
        {
            var sc = new Condition("Switch Condition",this);
            SwitchConditionGuid = sc.Guid;
        }
    }
}
