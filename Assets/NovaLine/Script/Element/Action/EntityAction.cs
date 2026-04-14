using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;
using UnityEngine;

namespace NovaLine.Script.Element.Action
{
    [Serializable]
    public class EntityAction : NovaAction
    {
        //Just an inspector tag
        public int entity = -1;

        [ShowInInspectorIf(nameof(entity),-1,ShowInInspectorIfAttribute.ValueCondition.MoreThan)]
        public bool animsOnly;
        [ShowInInspectorIf(nameof(animsOnly),false)]
        public TransformChecker startTransform;
        
        public List<NovaWrapper<EntityAnim>> anims;

        protected override IEnumerator OnInvoke()
        {
            var instantiatedEntity = EntityRegistry.GetInstantiatedEntity(entity);
            
            if(instantiatedEntity == null) yield break;
            
            if (!animsOnly)
            {
                instantiatedEntity.transform.localPosition = startTransform.position;
                instantiatedEntity.transform.localScale = startTransform.scale;
                instantiatedEntity.transform.localRotation = startTransform.rotation;
                instantiatedEntity.gameObject.SetActive(true);
            }
            
            yield return instantiatedEntity.AnimPlayer?.PlayAll(anims);
            
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Entity Action]";
        }
    }
}
