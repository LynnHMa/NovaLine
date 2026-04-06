using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    [Serializable]
    public class GetKeyEvent : NovaEvent
    {
        public KeyCode keyCode;
        public GetKeyEvent() { keyCode = KeyCode.None; }
        public GetKeyEvent(string name, KeyCode keyCode) : base(name)
        {
            this.keyCode = keyCode;
        }

        public override IEnumerator onEvent()
        {
            while (!Input.GetKey(keyCode))
            {
                yield return null;
                break;
            }
            yield return base.onEvent();
        }
        public override string getTypeName()
        {
            return "[Get Key Event]";
        }
    }
}
