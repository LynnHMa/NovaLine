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

            yield return null;
        }

        protected virtual IEnumerator OnPlay()
        {
            yield return null;
        }
    }

    public interface ILerpAnim
    {
        float Duration { get; }
    }
}