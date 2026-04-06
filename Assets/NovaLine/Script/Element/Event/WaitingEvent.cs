using System;
using System.Collections;
using System.Threading.Tasks;
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

        public override IEnumerator onEvent()
        {
            Debug.Log("Waiting Event");
            yield return new WaitForSeconds(second);
            
            yield return null;
            
            yield return base.onEvent();
        }

        public override string getTypeName()
        {
            return "[Waiting Event]";
        }
    }
}
