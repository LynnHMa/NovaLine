using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window.Command;
using NovaLine.Script.Element;
using UnityEditor;
using UnityEngine;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script.Editor.Window
{
    public static class InspectorHelper
    {
        private static ObjectInspectorWrapper wrapper;
        
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
                    wrapper = null;
                    elementJsonCache.Clear();
                    return;
                }

                wrapper = ObjectInspectorWrapper.CreateInstance(novaElement.guid);
                snapshotAllElements();
                Selection.activeObject = wrapper;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void OnInspectorObjValueChange()
        {
            if (wrapper == null) return;
            var parents = wrapper.parentElementGuidList;
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                var parentGuid = parents[i];
                var liveParent = FindElement(parentGuid);
                if (liveParent != null && TryRegisterChange(liveParent)) return;
            }

            var selected = FindElement(wrapper.selectedElementGuid);
            if (selected != null) TryRegisterChange(selected);
        }
        
        private static void snapshotAllElements()
        {
            elementJsonCache.Clear();
            if (wrapper == null) return;

            foreach (var guid in wrapper.parentElementGuidList)
            {
                var el = FindElement(guid);
                elementJsonCache[guid] = JsonUtility.ToJson(el);
            }
            
            var sel = FindElement(wrapper.selectedElementGuid);
            if (sel != null) elementJsonCache[sel.guid] = JsonUtility.ToJson(sel);
        }
        
        public static bool TryRegisterChange(NovaElement liveElement)
        {
            if (liveElement?.guid == null) return false;

            var currentJson = JsonUtility.ToJson(liveElement);

            var cachedJson = elementJsonCache.GetValueOrDefault(liveElement.guid, currentJson);

            if (currentJson == cachedJson) return false;

            var contextGuid = liveElement.parent?.guid ?? liveElement.guid;
            var contextType = liveElement.parent?.type ?? liveElement.type;
            
            string typeName = liveElement.GetType().AssemblyQualifiedName;
            
            CommandRegistry.Register(new InspectorElementChangeCommand(
                contextGuid, contextType,
                liveElement.guid, liveElement.type,
                cachedJson, currentJson,
                typeName, typeName));

            elementJsonCache[liveElement.guid] = currentJson;
            UpdateScope.RequireUpdate();
            return true;
        }
        
        public static void UpdateCacheForSwappedElement(NovaElement newLiveElement)
        {
            if (newLiveElement?.guid == null) return;
            elementJsonCache[newLiveElement.guid] = JsonUtility.ToJson(newLiveElement);
        }
    }
}