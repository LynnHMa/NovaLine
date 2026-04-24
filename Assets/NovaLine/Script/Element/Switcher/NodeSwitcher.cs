﻿using System;
 using System.Collections;
 using UnityEngine;
using static NovaLine.Script.Registry.NovaElementRegistry;

namespace NovaLine.Script.Element.Switcher
{
    [Serializable]
    public class NodeSwitcher : NovaSwitcher
    {
        [SerializeField,HideInInspector] private string _switchConditionGUID;
        
        public Condition SwitchCondition => FindElement(SwitchConditionGUID) as Condition;
        public string SwitchConditionGUID{ get => _switchConditionGUID; set => _switchConditionGUID = value; }
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
                clone.SwitchConditionGUID = SwitchCondition.Copy().GUID;
                clone.SwitchCondition.ParentGUID = clone.GUID;
            }
            
            return clone;
        }

        private void InitCondition()
        {
            var sc = new Condition("Switch Condition",this);
            SwitchConditionGUID = sc.GUID;
        }
    }
}
