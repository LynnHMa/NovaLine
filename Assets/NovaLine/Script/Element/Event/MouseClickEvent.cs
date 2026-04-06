using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    [Serializable]
    public class MouseClickEvent : NovaEvent
    {
        public int mouse;
        public MouseClickEvent() { mouse = 0; }
        public MouseClickEvent(string name, int mouse) : base(name)
        {
            this.mouse = mouse;
        }

        public override IEnumerator onEvent()
        {
            Debug.Log("Mouse click Event");
            while (!Input.GetMouseButtonDown(mouse))
            {
                yield return null;
            }

            yield return null;
            
            yield return base.onEvent();
        }
        public override string getTypeName()
        {
            return "[Mouse Click Event]";
        }
    }
}
