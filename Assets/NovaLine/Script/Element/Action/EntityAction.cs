using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Element.Action
{
    [Serializable]
    public class EntityAction : NovaAction
    {
        private int _entityIndex;
        
        //Just an inspector tag
        public int entity;
        public List<NovaWrapper<EntityAnim>> anims;

        public int entityIndex
        {
            get => _entityIndex;
            set => _entityIndex = value;
        }
        protected override IEnumerator onInvoke()
        {
            var instantiatedEntity = EntityRegistry.GetInstantiatedEntity(entityIndex);
            
            if(instantiatedEntity == null) yield break;

            instantiatedEntity.gameObject.SetActive(true);
            
            yield return instantiatedEntity.animPlayer?.playAll(anims);
            
            yield return base.onInvoke();
        }

        public override string getTypeName()
        {
            return "[Entity Action]";
        }

    }
}
