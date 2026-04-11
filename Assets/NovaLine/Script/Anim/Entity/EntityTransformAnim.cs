using System;
using System.Collections;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Anim.Entity
{
    [Serializable]
    public class EntityTransformAnim : EntityAnim,LerpAnim
    {
        public float duration;
        
        public bool move;
        [ShowInInspectorIf("move",true)]
        public Vector3 endPos;
        
        public bool scale;
        [ShowInInspectorIf("scale",true)]
        public Vector3 endScale;
        
        public bool rotate;
        [ShowInInspectorIf("rotate",true)]
        public Quaternion endRot;
        
        float LerpAnim.duration => duration;

        protected override IEnumerator onPlay()
        {
            if(linkedEntity == null) yield break;
            var timer = 0f;
            var startPos = linkedEntity.transform.localPosition;
            var startScale = linkedEntity.transform.localScale;
            var startRot = linkedEntity.transform.localRotation;
            while (timer < duration)
            {
                var t = timer / duration;
                if(move) linkedEntity.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                if(scale) linkedEntity.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                if(rotate) linkedEntity.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                timer += Time.deltaTime;
                yield return null;
            }
            if(move) linkedEntity.transform.localPosition = endPos;
            if(scale) linkedEntity.transform.localScale = endScale;
            if(rotate) linkedEntity.transform.localRotation = endRot;
            yield return base.onPlay();
        }
    }
}