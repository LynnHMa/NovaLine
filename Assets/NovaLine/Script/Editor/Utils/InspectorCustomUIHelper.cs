using NovaLine.Script.Element;
using System;
using System.Linq;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Command;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils
{
    public static class InspectorCustomUIHelper
    {
        public static void DrawTypeDropdown(SerializedProperty property, Type baseType, string label = "")
        {
            EditorGUILayout.Space(30);

            var derivedTypes = SubclassTypeHelper.GetSubTypes(baseType);

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
                        var beforeJson = JsonUtility.ToJson(oldElement);
                        JsonUtility.FromJsonOverwrite(beforeJson, newElement);
                        var afterJson = JsonUtility.ToJson(newElement);
                        
                        fixChildrenListReference(oldElement, newElement);
                        
                        var parentContextGuid = oldElement.parent != null ? oldElement.parent.guid : oldElement.guid;
                        var contextType = oldElement.parent != null ? oldElement.parent.type : oldElement.type;
                        
                        CommandRegistry.Register(new InspectorElementChangeCommand(
                            parentContextGuid, contextType,
                            oldElement.guid, oldElement.type,
                            beforeJson, afterJson,
                            oldElement.GetType().AssemblyQualifiedName,
                            newElement.GetType().AssemblyQualifiedName
                        ));
                        
                        NovaElementRegistry.ReplaceElement(oldElement.guid,newElement);
                        ContextRegistry.ReplaceLinkedElement(oldElement);
                        InspectorHelper.UpdateCacheForSwappedElement(newElement);
                        
                        property.managedReferenceValue = newElement;
                        property.serializedObject.ApplyModifiedProperties();
                        
                        refreshGraphNodeInfo();
                    }
                    else if (currentObj != null)
                    {
                        string beforeJson = JsonUtility.ToJson(currentObj);
                        JsonUtility.FromJsonOverwrite(beforeJson, newInstance);
                        property.managedReferenceValue = newInstance;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    else 
                    {
                        property.managedReferenceValue = newInstance;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"无法实例化类 {selectedType.Name}，请确保它有一个无参构造函数！\n{e.Message}");
                }
            }
        }
        private static void fixChildrenListReference(NovaElement oldElement, NovaElement newElement)
        {
            if (oldElement?.parent?.childrenGuidList == null)
                return;

            for (int i = 0; i < oldElement.parent.childrenGuidList.Count; i++)
            {
                oldElement.parent.childrenGuidList[i] = newElement.guid;
            }
        }

        private static void refreshGraphNodeInfo()
        {
            var graphView = ContextRegistry.CurrentGraphViewNodeContext?.graphView;
            if (graphView == null) return;

            graphView.update();
        }
    }
}