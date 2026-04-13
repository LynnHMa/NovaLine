using System;
using System.Collections;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script.Anim.Entity
{
    [Serializable]
    public class EntityTransformAnim : EntityAnim,ILerpAnim
    {
        public float duration;

        public TransformChecker startTransform;
        public TransformChecker endTransform;
        
        float ILerpAnim.Duration => duration;
        protected override IEnumerator OnPlay()
        {
            if(LinkedEntity == null) yield break;
            var timer = 0f;
            while (timer < duration)
            {
                var t = timer / duration;
                LinkedEntity.transform.localPosition = Vector3.Lerp(startTransform.position, endTransform.position, t);
                LinkedEntity.transform.localScale = Vector3.Lerp(startTransform.scale, endTransform.scale, t);
                LinkedEntity.transform.localRotation = Quaternion.Lerp(startTransform.rotation, endTransform.rotation, t);
                timer += Time.deltaTime;
                yield return null;
            }
            LinkedEntity.transform.localPosition = endTransform.position;
            LinkedEntity.transform.localScale = endTransform.scale;
            LinkedEntity.transform.localRotation = endTransform.rotation;
            yield return base.OnPlay();
        }
    }
}