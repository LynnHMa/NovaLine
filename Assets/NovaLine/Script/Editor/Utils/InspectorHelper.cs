using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Element;
using UnityEditor;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Utils
{
    public static class InspectorHelper
    {
        public static ObjectInspectorWrapper InspectorNovaElementWrapper { get; set; }
        
        private static readonly Dictionary<string, string> elementJsonCache = new();

        private static int _inspectorUpdateVersion = 0;

        public static async void ShowInInspector(this NovaElement novaElement)
        {
            try
            {
                int currentVersion = ++_inspectorUpdateVersion;

                await Task.Delay(50);

                if (currentVersion != _inspectorUpdateVersion) return;

                if (novaElement == null)
                {
                    Selection.activeObject = null;
                    InspectorNovaElementWrapper = null;
                    elementJsonCache.Clear();
                    return;
                }

                InspectorNovaElementWrapper = ObjectInspectorWrapper.CreateInstance(novaElement.Guid);
                snapshotAllElements();
                Selection.activeObject = InspectorNovaElementWrapper;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void OnInspectorObjValueChange()
        {
            if (InspectorNovaElementWrapper == null) return;
            var parents = InspectorNovaElementWrapper.parentElementGuidList;
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                var parentGuid = parents[i];
                var liveParent = FindElement(parentGuid);
                if (liveParent != null && TryRegisterChange(liveParent)) return;
            }

            var selected = FindElement(InspectorNovaElementWrapper.selectedElementGuid);
            if (selected != null) TryRegisterChange(selected);
        }
        
        private static void snapshotAllElements()
        {
            elementJsonCache.Clear();
            if (InspectorNovaElementWrapper == null) return;

            foreach (var guid in InspectorNovaElementWrapper.parentElementGuidList)
            {
                var el = FindElement(guid);
                elementJsonCache[guid] = JsonUtility.ToJson(el);
            }
            
            var sel = FindElement(InspectorNovaElementWrapper.selectedElementGuid);
            if (sel != null) elementJsonCache[sel.Guid] = JsonUtility.ToJson(sel);
        }
        
        public static bool TryRegisterChange(NovaElement liveElement)
        {
            if (liveElement?.Guid == null) return false;

            var currentJson = JsonUtility.ToJson(liveElement);

            var cachedJson = elementJsonCache.GetValueOrDefault(liveElement.Guid, currentJson);

            if (currentJson == cachedJson) return false;

            var contextGuid = liveElement.Parent?.Guid ?? liveElement.Guid;
            var contextType = liveElement.Parent?.Type ?? liveElement.Type;
            
            string typeName = liveElement.GetType().AssemblyQualifiedName;
            
            CommandRegistry.RegisterCommand(new InspectorElementChangeCommand(
                contextGuid, contextType,
                liveElement.Guid, liveElement.Type,
                cachedJson, currentJson,
                typeName, typeName));

            elementJsonCache[liveElement.Guid] = currentJson;
            UpdateScope.RequireUpdate();
            return true;
        }
        
        public static void UpdateCacheForSwappedElement(NovaElement newLiveElement)
        {
            if (newLiveElement?.Guid == null) return;
            elementJsonCache[newLiveElement.Guid] = JsonUtility.ToJson(newLiveElement);
        }
    }
}