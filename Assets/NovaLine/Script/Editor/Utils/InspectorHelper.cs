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
        public struct ElementSnapshot
        {
            public string Json { get;}
            public string TypeName { get; }

            public ElementSnapshot(string json, string typeName)
            {
                Json = json;
                TypeName = typeName;
            }
        }
        public static NovaElementInspectorWrapper InspectorNovaElementWrapper { get; set; }
        
        private static readonly Dictionary<string, ElementSnapshot> elementCache = new();

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
                    elementCache.Clear();
                    return;
                }

                InspectorNovaElementWrapper = NovaElementInspectorWrapper.CreateInstance(novaElement.Guid);
                SnapshotAllElements();
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
            var parents = InspectorNovaElementWrapper.ParentElementGuidList;
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                var parentGuid = parents[i];
                var liveParent = FindElement(parentGuid);
                if (liveParent != null && TryRegisterChange(liveParent)) return;
            }

            var selected = FindElement(InspectorNovaElementWrapper.SelectedElementGuid);
            if (selected != null) TryRegisterChange(selected);
        }
        
        private static void SnapshotAllElements()
        {
            elementCache.Clear();
            if (InspectorNovaElementWrapper == null) return;

            foreach (var el in InspectorNovaElementWrapper.parentElements)
            {
                if (el.Guid != null)
                {
                    elementCache[el.Guid] = new ElementSnapshot(JsonUtility.ToJson(el), el.GetType().AssemblyQualifiedName);
                }
            }

            var sel = InspectorNovaElementWrapper.selectedElement;
            if (sel.Guid != null)
            {
                elementCache[sel.Guid] = new ElementSnapshot(JsonUtility.ToJson(sel), sel.GetType().AssemblyQualifiedName);
            }
        }
        
        public static bool TryRegisterChange(NovaElement liveElement)
        {
            if (liveElement?.Guid == null) return false;

            string currentJson = JsonUtility.ToJson(liveElement);
            string currentTypeName = liveElement.GetType().AssemblyQualifiedName;
            
            if (!elementCache.TryGetValue(liveElement.Guid, out var cachedData))
            {
                cachedData = new ElementSnapshot(currentJson, currentTypeName);
            }
            
            if (currentJson == cachedData.Json && currentTypeName == cachedData.TypeName) 
                return false;

            var contextGuid = liveElement.Parent?.Guid ?? liveElement.Guid;
            var contextType = liveElement.Parent?.Type ?? liveElement.Type;
            
            CommandRegistry.RegisterCommand(new InspectorElementChangeCommand(
                contextGuid, contextType,
                liveElement.Guid, liveElement.Type,
                cachedData.Json, currentJson,
                cachedData.TypeName, currentTypeName));
            
            elementCache[liveElement.Guid] = new ElementSnapshot(currentJson, currentTypeName);
            UpdateScope.RequireUpdate();
            return true;
        }
    }
}