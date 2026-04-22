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
            
            var guids = AssetDatabase.FindAssets($"{assetName} t:{typeFilter}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[AssetDatabaseExt] 找不到名为 '{assetName}' 且类型匹配的文件！");
                return null;
            }
            
            var realPath = AssetDatabase.GUIDToAssetPath(guids[0]);
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