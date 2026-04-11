using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script
{
    public class Entity : MonoBehaviour
    {
        public EntityAnimPlayer animPlayer { get; set; }
        public SpriteRenderer spriteRenderer { get; set; }
        public new string name;
        public string description;

        private void Awake()
        {
            animPlayer = new(this);
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    public class EntityAnimPlayer
    {
        public Entity linkedEntity { get; }

        public EntityAnimPlayer(Entity linkedEntity)
        {
            this.linkedEntity = linkedEntity;
        }

        public IEnumerator playAll(List<NovaWrapper<EntityAnim>> anims)
        {
            for (var i = 0; i < anims.Count; i++)
            {
                var animWrapper = anims[i];
                var anim = animWrapper.value;
                anim.linkedEntity = linkedEntity;
                yield return anim?.play();
            }
        }
    }
}