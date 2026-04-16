using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Element.Event
{
    [Serializable]
    public class WaitingEvent : NovaEvent
    {
        public int second;
        public WaitingEvent() { second = 1; }
        public WaitingEvent(string name, int second) : base(name)
        {
            this.second = second;
        }

        public override IEnumerator OnEvent()
        {
            yield return new WaitForSeconds(second);
            
            yield return null;
            
            yield return base.OnEvent();
        }

        public override string GetTypeName()
        {
            return "[Waiting Event]";
        }
    }
}
