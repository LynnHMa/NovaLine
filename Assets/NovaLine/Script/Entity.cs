using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Interface.Debounce;
using UnityEngine;

namespace NovaLine.Script
{
    public class Entity : MonoBehaviour,IGameObjectActiveDebounce
    {
        public EntityAnimPlayer AnimPlayer { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
        public Coroutine HideShowCoroutine { get; set; }
        public new string name;
        public string description;
        private void Awake()
        {
            AnimPlayer = new(this);
            SpriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        }
        
        public void InactiveDebounce()
        {
            var player = NovaPlayer.Instance;
            if (player == null) return;
            if(this != null && HideShowCoroutine != null) player.StopCoroutine(HideShowCoroutine);
            HideShowCoroutine = player.StartCoroutine(HideShowDebounceRoutine(false));
        }

        public void ActiveDebounce()
        {
            var player = NovaPlayer.Instance;
            if (player == null) return;
            if(this != null && HideShowCoroutine != null) player.StopCoroutine(HideShowCoroutine);
            HideShowCoroutine = player.StartCoroutine(HideShowDebounceRoutine(true));
        }

        public IEnumerator HideShowDebounceRoutine(bool isActive)
        {
            yield return new WaitForSeconds(0.05f);
            if (this == null || gameObject == null) yield break;
            gameObject.SetActive(isActive);
        }
    }
    
    public class EntityAnimPlayer
    {
        public Entity LinkedEntity { get; }

        public EntityAnimPlayer(Entity linkedEntity)
        {
            LinkedEntity = linkedEntity;
        }

        public IEnumerator PlayAll(List<NovaWrapper<EntityAnim>> anims)
        {
            for (var i = 0; i < anims.Count; i++)
            {
                var animWrapper = anims[i];
                var anim = animWrapper.Value;
                anim.LinkedEntity = LinkedEntity;
                yield return anim?.Play();
            }
        }
    }
}