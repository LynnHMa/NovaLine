using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Anim.Entity
{
    [Serializable]
    public class EntityFadeAnim : EntityAnim,ILerpAnim
    {
        public bool fadeIn = true;
        public float duration = 1f;
        
        float ILerpAnim.Duration => duration;
        protected override IEnumerator OnPlay()
        {
            LinkedEntity.ActiveDebounce();
            
            var spriteRenderer = LinkedEntity?.SpriteRenderer;
            if(spriteRenderer == null) yield break;
            
            var timer = fadeIn ? 0f : duration;
            
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, timer / duration);
            spriteRenderer.gameObject.SetActive(true);
            
            while (fadeIn ? timer < duration : timer > 0f)
            {
                var alpha = timer / duration;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
                timer += Time.deltaTime * (fadeIn ? 1f : -1f);
                yield return null;
            }
            
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, fadeIn ? 1f : 0f);
            
            yield return base.OnPlay();
        }
    }
}