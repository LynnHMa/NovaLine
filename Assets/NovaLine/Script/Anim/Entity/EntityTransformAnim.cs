using System;
using System.Collections;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Anim.Entity
{
    [Serializable]
    public class EntityTransformAnim : EntityAnim,ILerpAnim
    {
        public float duration;
        
        public bool move;
        [ShowInInspectorIf("move",true)]
        public Vector3 startPos;
        [ShowInInspectorIf("move",true)]
        public Vector3 endPos;
        
        public bool scale;
        [ShowInInspectorIf("move",true)]
        public Vector3 startScale;
        [ShowInInspectorIf("scale",true)]
        public Vector3 endScale;
        
        public bool rotate;
        [ShowInInspectorIf("move",true)]
        public Quaternion startRot;
        [ShowInInspectorIf("rotate",true)]
        public Quaternion endRot;
        
        float ILerpAnim.Duration => duration;

        protected override IEnumerator OnPlay()
        {
            if(LinkedEntity == null) yield break;
            var timer = 0f;
            while (timer < duration)
            {
                var t = timer / duration;
                if(move) LinkedEntity.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                if(scale) LinkedEntity.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                if(rotate) LinkedEntity.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                timer += Time.deltaTime;
                yield return null;
            }
            if(move) LinkedEntity.transform.localPosition = endPos;
            if(scale) LinkedEntity.transform.localScale = endScale;
            if(rotate) LinkedEntity.transform.localRotation = endRot;
            yield return base.OnPlay();
        }
    }
}