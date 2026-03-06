using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Event
{
    [Serializable]
    public class GetKeyEvent : NovaEvent
    {
        public KeyCode keyCode;
        public GetKeyEvent(KeyCode keyCode)
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
        public override string getType()
        {
            return "[Get Key Event]";
        }
    }
}
