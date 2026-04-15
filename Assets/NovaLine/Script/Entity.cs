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
        public EntityAnimPlayer AnimPlayer { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
        public new string name;
        public string description;

        private void Awake()
        {
            AnimPlayer = new(this);
            SpriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
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