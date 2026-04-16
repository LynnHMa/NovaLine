using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Anim
{
    [Serializable]
    public class NovaAnim
    {
        public float waitingSecondsBeforePlay;
        public float waitingSecondsAfterPlay;
        
        protected NovaAnim(){}
        
        public virtual IEnumerator Play()
        {
            yield return new WaitForSeconds(waitingSecondsBeforePlay);
            
            yield return OnPlay();
            
            yield return new WaitForSeconds(waitingSecondsAfterPlay);
        }

        protected virtual IEnumerator OnPlay()
        {
            yield break;
        }
    }

    public interface ILerpAnim
    {
        float Duration { get; }
    }
}