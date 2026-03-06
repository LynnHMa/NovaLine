using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Event
{
    [Serializable]
    public class MouseClickEvent : NovaEvent
    {
        public readonly static MouseClickEvent LEFT_CLICK = new MouseClickEvent(0);
        public readonly static MouseClickEvent RIGHT_CLICK = new MouseClickEvent(1);
        public readonly static MouseClickEvent MIDDLE_CLICK = new MouseClickEvent(2);

        public int mouse;
        public MouseClickEvent(int mouse)
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
        public override string getType()
        {
            return "[Mouse Click Event]";
        }
    }
}
