using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    /// <summary>
    /// Only listen for mouse down events under all circumstances.
    /// Override for custom listening logic.
    /// </summary>
    [Serializable]
    public class MouseClickEvent : NovaEvent
    {
        public int mouse;
        public MouseClickEvent() { mouse = 0; }
        public MouseClickEvent(string name, int mouse) : base(name)
        {
            this.mouse = mouse;
        }
        
        public override IEnumerator OnEvent()
        {
            while (!Input.GetMouseButtonDown(mouse))
            {
                yield return null;
            }
            yield return null;
            
            yield return base.OnEvent();
        }
        public override string GetTypeName()
        {
            return "[Mouse Click Event]";
        }
    }
}
