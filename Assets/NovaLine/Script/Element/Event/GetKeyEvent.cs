using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    /// <summary>
    /// Only listen for key down events under all circumstances.
    /// Override for custom listening logic.
    /// </summary>
    [Serializable]
    public class GetKeyEvent : NovaEvent
    {
        public KeyCode keyCode;
        public GetKeyEvent() { keyCode = KeyCode.None; }
        public GetKeyEvent(string name, KeyCode keyCode) : base(name)
        {
            this.keyCode = keyCode;
        }
        
        public override IEnumerator OnEvent()
        {
            while (!Input.GetKey(keyCode))
            {
                yield return null;
            }
            
            yield return null;
            
            yield return base.OnEvent();
        }
        public override string GetTypeName()
        {
            return "[Get Key Event]";
        }
    }
}
