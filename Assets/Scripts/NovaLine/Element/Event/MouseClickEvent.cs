using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Element.Event
{
    [Serializable]
    public class MouseClickEvent : NovaEvent
    {
        public int mouse;
        public MouseClickEvent() : base()
        {
            mouse = 0;
        }
        public MouseClickEvent(int mouse) : base()
        {
            this.mouse = mouse;
        }
        public override async Task onEvent()
        {
            while (!Input.GetMouseButtonDown(mouse))
            {
                await Task.Yield();
            }
            await base.onEvent();
        }
        public override string getTypeName()
        {
            return "[Mouse Click Event]";
        }
    }
}
