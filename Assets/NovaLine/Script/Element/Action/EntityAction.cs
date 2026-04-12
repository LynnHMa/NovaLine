using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    [Serializable]
    public class EntityAction : NovaAction
    {
        private int _entityIndex;
        
        //Just an inspector tag
        public int entity = -1;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public List<NovaWrapper<EntityAnim>> anims;

        public int EntityIndex
        {
            get => _entityIndex;
            set => _entityIndex = value;
        }
        protected override IEnumerator OnInvoke()
        {
            var instantiatedEntity = EntityRegistry.GetInstantiatedEntity(EntityIndex);
            
            if(instantiatedEntity == null) yield break;

            instantiatedEntity.gameObject.SetActive(true);
            
            yield return instantiatedEntity.AnimPlayer?.PlayAll(anims);
            
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Entity Action]";
        }

    }
}
