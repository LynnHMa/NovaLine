using NovaLine.Action;
using NovaLine.Element;
using System;
using System.Linq;
using NovaLine.Editor.Window;
using NovaLine.Editor.Window.Command;
using NovaLine.Element.Event;
using UnityEditor;
using UnityEngine;
using static NovaLine.Editor.Window.WindowContextRegistry;

namespace NovaLine.Editor.Utils
{
    public static class SerializeReferenceUI
    {
        public static void DrawTypeDropdown(SerializedProperty property, Type baseType, string label = "Type")
        {
            EditorGUILayout.Space(30);

            var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToList();

            derivedTypes.Reverse();

            if (derivedTypes.Count == 0) return;

            string[] typeNames = new string[derivedTypes.Count];
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                typeNames[i] = derivedTypes[i].Name;
            }

            int currentIndex = 0;
            object currentObj = property.managedReferenceValue;
            if (currentObj != null)
            {
                Type currentType = currentObj.GetType();
                int typeIndex = derivedTypes.IndexOf(currentType);
                if (typeIndex >= 0)
                {
                    currentIndex = typeIndex;
                }
            }

            int newIndex = EditorGUILayout.Popup(label, currentIndex, typeNames);

            if (newIndex != currentIndex)
            {
                Type selectedType = derivedTypes[newIndex];
                try
                {
                    object newInstance = Activator.CreateInstance(selectedType);

                    if (currentObj is NovaElement oldElement && newInstance is NovaElement newElement)
                    {
                        newElement.name = oldElement.name;
                        newElement.guid = oldElement.guid;
                        newElement.describtion = oldElement.describtion;
                        newElement.parent = oldElement.parent;
                        
                        if (oldElement is NovaAction oldAction && newElement is NovaAction newAction)
                        {
                            newAction.conditionAfterInvoke = oldAction.conditionAfterInvoke;
                            newAction.conditionBeforeInvoke = oldAction.conditionBeforeInvoke;
                        }

                        newElement.ReplaceToContext();

                        property.managedReferenceValue = newElement;
                        property.serializedObject.ApplyModifiedProperties();
                        
                        CommandRegistry.Register(new InspectorElementChangeCommand(newElement.parent.guid,newElement.parent.type,oldElement,newElement));
                        InspectorHelper.UpdateCache();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"无法实例化类 {selectedType.Name}，请确保它有一个无参构造函数！\n{e.Message}");
                }
            }
        }
    }
}