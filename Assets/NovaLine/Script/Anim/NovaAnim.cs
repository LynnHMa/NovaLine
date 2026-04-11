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
        
        public virtual IEnumerator play()
        {
            yield return new WaitForSeconds(waitingSecondsBeforePlay);
            
            yield return onPlay();
            
            yield return new WaitForSeconds(waitingSecondsAfterPlay);

            yield return null;
        }

        protected virtual IEnumerator onPlay()
        {
            yield return null;
        }
    }

    public interface LerpAnim
    {
        float duration { get; }
    }
}