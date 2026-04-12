using System.Reflection;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element.Action;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Attribute;
using NovaLine.Script.Utils.Interface;
using Unity.VisualScripting;

namespace NovaLine.Script.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Script.Element;
    using NovaLine.Script.Action;
    using System;
    using static NovaLine.Script.Editor.Window.ContextRegistry;

    [CustomEditor(typeof(ObjectInspectorWrapper))]
    public class ObjectInspectorWrapperEditor : Editor
    {
        private static GUIStyle SELECTED_ELEMENT_STYLE;

        private static GUIStyle SELECTED_PARENT_ELEMENT_STYLE;

        private static GUIStyle ARROW_STYLE;

        private void OnEnable()
        {
            SELECTED_ELEMENT_STYLE = new GUIStyle()
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = ColorExt.NODE_THEMED_COLOR },
                alignment = TextAnchor.MiddleCenter
            };

            SELECTED_PARENT_ELEMENT_STYLE = new GUIStyle()
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = ColorExt.ACTION_THEMED_COLOR },
                alignment = TextAnchor.MiddleCenter
            };

            ARROW_STYLE = new GUIStyle()
            {
                fontSize = 35,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = ColorExt.ACTION_THEMED_COLOR },
                alignment = TextAnchor.MiddleCenter
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ModifyInfo();

            serializedObject.ApplyModifiedProperties();

            InterceptUndoRedo();
        }

        private void ModifyInfo()
        {
            var parentsProp = serializedObject.FindProperty("parentElements");
            var selectedProp = serializedObject.FindProperty("selectedElement");

            //Draw parent elements
            if (parentsProp.arraySize > 0)
            {
                EditorGUILayout.Space(15);

                for (int i = 0; i < parentsProp.arraySize; i++)
                {
                    var parentItemProp = parentsProp.GetArrayElementAtIndex(i);

                    DrawElement(parentItemProp, SELECTED_PARENT_ELEMENT_STYLE);

                    EditorGUILayout.LabelField("↓", ARROW_STYLE);
                }
            }

            //Draw selected element
            DrawElement(selectedProp, SELECTED_ELEMENT_STYLE);
        }

        private static void LoadConditionContextDirect(Condition targetCondition, string fallbackName)
        {
            if (targetCondition == null)
            {
                Debug.LogWarning("Can't load condition context ,because Condition is null!");
                return;
            }

            var context = GetContext(targetCondition.Guid, NovaElementType.CONDITION);
            if (context is ConditionContext conditionContext)
            {
                var actualConditionName = targetCondition.name;

                if (string.IsNullOrEmpty(actualConditionName))
                {
                    actualConditionName = fallbackName;
                }

                targetCondition.name = actualConditionName;
                NovaWindow.LoadContextInWindow(conditionContext);
            }
        }

        private static void DrawElement(SerializedProperty selectedProp, GUIStyle style)
        {
            if (selectedProp == null) return;
            if (selectedProp.managedReferenceValue is NovaElement selectedElement)
            {
                EditorGUILayout.Space(30);

                EditorGUILayout.LabelField(selectedElement.GetActualName(), style);

                switch (selectedElement)
                {
                    case NovaAction:
                        InspectorCustomUIHelper.DrawTypeDropdown(selectedProp, typeof(INovaAction), "Action Type");
                        break;
                    case NovaEvent:
                        InspectorCustomUIHelper.DrawTypeDropdown(selectedProp, typeof(INovaEvent), "Event Type");
                        break;
                }

                selectedProp.isExpanded = true;

                SerializedProperty iterator = selectedProp.Copy();
                DrawProperty(iterator,selectedElement);

                EditorGUILayout.Space(15);
                if (selectedElement is IAroundConditionElement or NodeSwitcher)
                {
                    void DrawConditionUI(string label, Condition conditionObj, string fallbackName)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        string displayName = conditionObj != null ? conditionObj.GetActualName() : "Null / 未分配";
                        EditorGUILayout.TextField(displayName);
                        GUI.enabled = true;
                        if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(20)))
                        {
                            LoadConditionContextDirect(conditionObj, fallbackName);
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    switch (selectedElement)
                    {
                        case IAroundConditionElement conditionElement:
                            DrawConditionUI("Condition Before Invoke", conditionElement.ConditionBeforeInvoke,
                                $"Before Invoke");
                            EditorGUILayout.Space(10);
                            DrawConditionUI("Condition After Invoke", conditionElement.ConditionAfterInvoke,
                                $"After Invoke");
                            break;
                        case NodeSwitcher nodeSwitcher:
                            DrawConditionUI("Switch Condition", nodeSwitcher.switchCondition, "Switch Condition");
                            EditorGUILayout.Space(10);
                            break;
                    }
                }

                if (NovaWindow.SelectedGraphNode != null &&
                    NovaWindow.SelectedGraphNode.linkedElement.Guid.Equals(selectedElement.Guid) &&
                    NovaWindow.SelectedGraphNode.inputContainer.childCount != 0 &&
                    NovaWindow.SelectedGraphNode.outputContainer.childCount != 0)
                {
                    EditorGUILayout.Space(30);
                    if (GUILayout.Button("Set To Start", GUILayout.Height(30)))
                    {
                        var currentGraphView = CurrentGraphViewNodeContext?.GraphView;
                        if (currentGraphView != null)
                        {
                            currentGraphView.SetFirstNode(NovaWindow.SelectedGraphNode);
                            currentGraphView.Update();
                            SaveScope.RequireSave();
                        }
                    }

                    EditorGUILayout.Space(30);
                }
            }
        }

        private static void DrawEntityDropDown(SerializedProperty prop,object actualParentObject)
        {
            if (actualParentObject is not EntityAction entityAction || entityAction.Parent.Parent is not Flowchart flowchart) return;
            var entityPrefabs = flowchart.entityPrefabs;
            var entityDisplayNameList = new string[entityPrefabs.Count];
            for (var i = 0; i < entityPrefabs.Count; i++)
            {
                entityDisplayNameList[i] = $"[{i}] {entityPrefabs[i].name}";
            }
            var newIndex = EditorGUILayout.Popup("Entity",prop.intValue,entityDisplayNameList);
            Undo.RecordObject(prop.serializedObject.targetObject, "Set Entity");
            entityAction.EntityIndex = newIndex;
            prop.intValue = newIndex;
            prop.serializedObject.ApplyModifiedProperties();
        }
        private static void DrawProperty(SerializedProperty iterator, object actualParentObject)
        {
            SerializedProperty endProperty = iterator.GetEndProperty();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty))
                        break;
                    
                    if (ShouldShow(iterator, actualParentObject))
                    {
                        EditorGUILayout.Space(10);

                        switch (iterator.name)
                        {
                            case "anims" when iterator.isArray:
                                DrawAnimList(iterator);
                                break;
                            case "entity":
                                DrawEntityDropDown(iterator,actualParentObject);
                                break;
                            default:
                                EditorGUILayout.PropertyField(iterator, true);
                                break;
                        }
                    }

                } while (iterator.NextVisible(false));
            }
        }
        private static void DrawAnimList(SerializedProperty listProp)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Anim List", EditorStyles.boldLabel);

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var valueProp = element.FindPropertyRelative("_value"); 

                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField($"Anim {i}", GUILayout.Width(50));

                if (valueProp != null)
                {
                    InspectorCustomUIHelper.DrawTypeDropdown(valueProp, typeof(EntityAnim));
                }

                GUILayout.FlexibleSpace(); 
                
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(30)))
                {
                    GUI.backgroundColor = Color.white;
                    listProp.DeleteArrayElementAtIndex(i);
                    listProp.serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                if (valueProp?.managedReferenceValue != null)
                {
                    valueProp.isExpanded = true; 

                    EditorGUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    
                    DrawProperty(valueProp.Copy(), valueProp.managedReferenceValue);
                    
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+ Add Anim", GUILayout.Height(30)))
            {
                ShowAddAnimMenu(listProp); 
            }
            GUI.backgroundColor = Color.white;
        }
        private static void ShowAddAnimMenu(SerializedProperty listProp)
        {
            var menu = new GenericMenu();
            var types = SubclassTypeHelper.GetSubTypes(typeof(EntityAnim));
            string path = listProp.propertyPath;
            var so = listProp.serializedObject;

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    so.Update();
                    var list = so.FindProperty(path);
                    Undo.RecordObject(so.targetObject, "Add Anim");
                    list.arraySize++;
                    var newElement = list.GetArrayElementAtIndex(list.arraySize - 1);
                    var value = newElement.FindPropertyRelative("_value");
                    if (value != null) value.managedReferenceValue = Activator.CreateInstance(type);
                    so.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }
        private static void InterceptUndoRedo()
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.isKey)
            {
                if (EditorGUIUtility.editingTextField)
                {
                    return;
                }

                switch (e.keyCode)
                {
                    case KeyCode.Z:
                    {
                        if (e.shift)
                        {
                            CommandRegistry.Redo();
                        }
                        else
                        {
                            CommandRegistry.Undo();
                        }

                        e.Use();
                        break;
                    }
                    case KeyCode.Y:
                        CommandRegistry.Redo();
                        e.Use();
                        break;
                }
            }
        }
        private static bool ShouldShow(SerializedProperty property, object actualParentObject)
        {
            if (actualParentObject == null) return true;
            
            FieldInfo fieldInfo = actualParentObject.GetType().GetField(
                property.name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo == null) return true;
            
            var attr = fieldInfo.GetCustomAttribute<ShowInInspectorIfAttribute>();
            if (attr == null) return true;

            if (attr.HideInEditMode && !Application.isPlaying) return false;
            
            string conditionPath = property.propertyPath.Replace(property.name, attr.ConditionField);
            SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

            if (conditionProp != null)
            {
                if (conditionProp.propertyType == SerializedPropertyType.Boolean)
                {
                    return conditionProp.boolValue.Equals(attr.ExpectedValue);
                }
            }
            
            var conditionField = actualParentObject.GetType().GetField(
                attr.ConditionField,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (conditionField != null)
            {
                var value = conditionField.GetValue(actualParentObject);
                return Equals(value, attr.ExpectedValue);
            }

            return true;
        }
    }
}
