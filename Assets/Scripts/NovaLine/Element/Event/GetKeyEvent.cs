using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Element.Event
{
    [Serializable]
    public class GetKeyEvent : NovaEvent
    {
        public KeyCode keyCode;
        public GetKeyEvent() : base()
        {
            keyCode = KeyCode.None;
        }
        public GetKeyEvent(KeyCode keyCode) : base()
        {
            this.keyCode = keyCode;
        }
        public override async Task onEvent()
        {
            while (!Input.GetKey(keyCode))
            {
                await Task.Yield();
            }
            await base.onEvent();
        }
        public override string getTypeName()
        {
            return "[Get Key Event]";
        }
    }
}
