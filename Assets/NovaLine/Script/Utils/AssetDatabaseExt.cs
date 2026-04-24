using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Utils
{
    public static class AssetDatabaseExt
    {
        public static T LoadAssetByName<T>(string assetName) where T : Object
        {
            var typeFilter = typeof(T).Name;
            var isComponent = typeof(Component).IsAssignableFrom(typeof(T));
            if (isComponent)
            {
                typeFilter = "GameObject"; 
            }
            
            var GUIDs = AssetDatabase.FindAssets($"{assetName} t:{typeFilter}");
            if (GUIDs.Length == 0)
            {
                Debug.LogWarning($"[AssetDatabaseExt] Can't find asset named {assetName} !");
                return null;
            }
            
            var realPath = AssetDatabase.GUIDToAssetPath(GUIDs[0]);
            if (isComponent)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(realPath);
                return go != null ? go.GetComponent<T>() : null;
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<T>(realPath);
            }
        }
    }
}