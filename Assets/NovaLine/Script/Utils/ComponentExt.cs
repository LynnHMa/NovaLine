using System;
using UnityEditorInternal;
using UnityEngine;

namespace NovaLine.Script.Utils
{
    public static class ComponentExt
    {
        #if UNITY_EDITOR
        public static Component CopyComponent(this Component from, GameObject targetObject)
        {
            if (from == null || targetObject == null) return null;
            
            Type realType = from.GetType();
            
            Component to = targetObject.GetComponent(realType);
            if (to == null)
            {
                to = targetObject.AddComponent(realType);
            }
            
            ComponentUtility.CopyComponent(from);
            ComponentUtility.PasteComponentValues(to);

            return to;
        }
        #endif
    }
}