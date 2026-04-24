using System.Collections.Generic;
using UnityEngine;

namespace NovaLine.Script.Registry
{
    public static class EntityRegistry
    {
        public static List<Entity> InstantiatedEntities { get; } = new();

        public static Entity RegisterEntity(Entity entityPrefab)
        {
            if (entityPrefab == null) return null;
            var entity = Object.Instantiate(entityPrefab, NovaPlayer.Instance.entityStorage, true);
            entity.SpriteRenderer.sortingLayerName = NovaPlayer.Instance.defaultEntitySortingLayer;
            entity.SpriteRenderer.sortingOrder = NovaPlayer.Instance.defaultEntityOrderLayer;
            InstantiatedEntities.Add(entity);
            entity.gameObject.SetActive(false);
            return entity;
        }
        
        public static void UnregisterEntity(Entity entity)
        {
            InstantiatedEntities.Remove(entity);
            if (entity == null) return;
            Object.Destroy(entity.gameObject);
        }

        public static void UnregisterEntity(int index)
        {
            var entity = GetInstantiatedEntity(index);
            UnregisterEntity(entity);
        }
        
        public static Entity GetInstantiatedEntity(int index)
        {
            return index >= InstantiatedEntities.Count ? null : InstantiatedEntities[index];
        }
        
        public static void ClearEntities()
        {
            foreach (var entity in InstantiatedEntities)
            {
                if(entity == null) continue;
                Object.Destroy(entity.gameObject);
            }
            InstantiatedEntities.Clear();
        }
    }
}