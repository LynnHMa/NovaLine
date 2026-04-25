using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Registry;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;

namespace NovaLine.Script.Element.Action
{
    /// <summary>
    /// Manages an entity (character or object).
    /// Shows the entity or plays its animation.
    /// </summary>
    [Serializable]
    public class EntityAction : NovaAction
    {
        public enum EntityActionType
        {
            ShowDirectly,
            Anim
        }
        //Just an inspector tag
        public int entity = -1;
        public EntityActionType entityActionType = EntityActionType.ShowDirectly;

        [ShowInInspectorIf(nameof(entityActionType), EntityActionType.ShowDirectly)]
        [ShowInInspectorIf(nameof(entity),-1,ShowInInspectorIfAttribute.ValueCondition.MoreThan)]
        public TransformChecker showTransform;
        
        [ShowInInspectorIf(nameof(entityActionType), EntityActionType.Anim)]
        [ShowInInspectorIf(nameof(entity),-1,ShowInInspectorIfAttribute.ValueCondition.MoreThan)]
        public List<NovaWrapper<EntityAnim>> anims;

        protected override IEnumerator OnInvoke()
        {
            var instantiatedEntity = EntityRegistry.GetInstantiatedEntity(entity);

            switch (entityActionType)
            {
                case EntityActionType.ShowDirectly:
                    showTransform.LinkedTransform = instantiatedEntity.transform;
                    showTransform.ExportToTransform();
                    instantiatedEntity.ActiveDebounce();
                    break;
                case EntityActionType.Anim:
                    yield return instantiatedEntity?.AnimPlayer?.PlayAll(anims);
                    break;
            }
            
            
            yield return base.OnInvoke();
        }

        public override string GetTypeName()
        {
            return "[Entity Action]";
        }
    }
}
