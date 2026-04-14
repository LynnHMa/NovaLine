using System;
using System.Collections.Generic;
using System.Reflection;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Action;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Attribute;
using NovaLine.Script.Utils.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovaLine.Script.Editor.Utils.OverrideEditor
{
    using static Window.ContextRegistry;

    [UnityEditor.CustomEditor(typeof(NovaElementInspectorWrapper))]
    public class NovaElementInspectorWrapperEditor : UnityEditor.Editor
    {
        private static GUIStyle SELECTED_ELEMENT_STYLE;

        private static GUIStyle SELECTED_PARENT_ELEMENT_STYLE;

        private static GUIStyle ARROW_STYLE; 
        
        private static readonly Dictionary<string, Vector2> _scrollPositionCache = new();
        
        private ScrollView _inspectorScrollView;
        private string _currentElementGuid;
        private bool _hasRestoredScroll;
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
            
            var wrapper = target as NovaElementInspectorWrapper;
            _currentElementGuid = wrapper?.selectedElement?.Guid;
            _hasRestoredScroll = false;
            EditorApplication.delayCall += HookInspectorScrollView;
        }

        public override void OnInspectorGUI()
        {
            if (_inspectorScrollView != null && !string.IsNullOrEmpty(_currentElementGuid))
            {
                if (!_hasRestoredScroll)
                {
                    if (_scrollPositionCache.TryGetValue(_currentElementGuid, out Vector2 savedPos))
                    {
                        _inspectorScrollView.scrollOffset = savedPos;
                        if (_inspectorScrollView.contentContainer.layout.height >= savedPos.y)
                        {
                            _hasRestoredScroll = true;
                        }
                    }
                    else
                    {
                        _hasRestoredScroll = true;
                    }
                }
                else
                {
                    _scrollPositionCache[_currentElementGuid] = _inspectorScrollView.scrollOffset;
                }
            }
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
        private void HookInspectorScrollView()
        {
            if (target == null) return;
            
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window.GetType().Name == "InspectorWindow")
                {
                    var scrollView = window.rootVisualElement?.Q<ScrollView>();
                    if (scrollView != null)
                    {
                        _inspectorScrollView = scrollView;
                        break;
                    }
                }
            }
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
                
                selectedProp.isExpanded = true;

                //Add drop down of modifying related type
                switch (selectedElement)
                {
                    case NovaAction:
                        DrawTypeDropdown(selectedProp, typeof(INovaAction), "Action Type");
                        break;
                    case NovaEvent:
                        DrawTypeDropdown(selectedProp, typeof(INovaEvent), "Event Type");
                        break;
                }
                
                DrawProperty(selectedProp,selectedElement);

                EditorGUILayout.Space(15);
                
                //Add a button of entering related condition graph view
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
                        
                        GUI.backgroundColor = ColorExt.EVENT_THEMED_COLOR;
                        if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(20)))
                        {
                            LoadConditionContextDirect(conditionObj, fallbackName);
                        }
                        GUI.backgroundColor = Color.white;

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
                    
                    EditorGUILayout.Space(15);
                }
                
                //Add a button of setting first node 
                if (NovaWindow.SelectedGraphNode != null &&
                    NovaWindow.SelectedGraphNode.linkedElement.Guid.Equals(selectedElement.Guid) &&
                    NovaWindow.SelectedGraphNode.inputContainer.childCount != 0 &&
                    NovaWindow.SelectedGraphNode.outputContainer.childCount != 0)
                {
                    EditorGUILayout.Space(30);
                    
                    GUI.backgroundColor = NovaWindow.SelectedGraphNode.ThemedColor;
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
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.Space(30);
                }
            }
        }
        private static void DrawTypeDropdown(SerializedProperty property, Type baseType, string label = "")
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
            object propObj = property.managedReferenceValue;
            if (propObj != null)
            {
                Type currentType = propObj.GetType();
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
                object newInstance = Activator.CreateInstance(selectedType);
                try
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Change Type");
                    if (propObj is NovaElement oldElement && newInstance is NovaElement newElement)
                    {
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(oldElement), newElement);

                        if (oldElement.Parent.ChildrenGuidList != null)
                        {
                            for (int i = 0; i < oldElement.Parent.ChildrenGuidList.Count; i++)
                            {
                                oldElement.Parent.ChildrenGuidList[i] = newElement.Guid;
                            }
                        }
                        
                        NovaElementRegistry.ReplaceElement(oldElement.Guid,newElement);
                        ReplaceLinkedElementInContext(oldElement);
                        
                        property.managedReferenceValue = newElement;
                    }
                    else if (propObj != null)
                    {
                        string beforeJson = JsonUtility.ToJson(propObj);
                        JsonUtility.FromJsonOverwrite(beforeJson, newInstance);
                        property.managedReferenceValue = newInstance;
                    }
                    else 
                    {
                        property.managedReferenceValue = newInstance;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }
                catch (Exception e)
                {
                    Debug.LogError($"实例化失败：\n{e.Message}");
                }
            }
        }
        private static void DrawEntitySelectorDropDown(SerializedProperty prop,object actualParentObject)
        {
            if (actualParentObject is not EntityAction entityAction || entityAction.Parent.Parent is not Flowchart flowchart) return;
            var entityPrefabs = flowchart.entityPrefabs;
            var entityDisplayNameList = new List<string>();
            for (var i = 0; i < entityPrefabs.Count; i++)
            {
                var prefab = entityPrefabs[i];
                if(prefab == null) continue;
                entityDisplayNameList.Add($"[{i}] {entityPrefabs[i].name}");
            }
            var entityDisplayNameArray = entityDisplayNameList.ToArray();
            var newIndex = entityDisplayNameArray.Length == 0 ? -1 : EditorGUILayout.Popup("Entity",prop.intValue,entityDisplayNameArray);
            Undo.RecordObject(prop.serializedObject.targetObject, "Set Entity");
            entityAction.entity = newIndex;
            prop.intValue = newIndex;
            prop.serializedObject.ApplyModifiedProperties();
        }
        private static void DrawProperty(SerializedProperty selectedProp, object actualParentObject)
        {
            SerializedProperty endChildProp = selectedProp.GetEndProperty();
            if (selectedProp.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(selectedProp, endChildProp))
                        break;
                    
                    if (ShouldShow(selectedProp, actualParentObject))
                    {
                        selectedProp.isExpanded = true;
                        
                        EditorGUILayout.Space(10);

                        var isEntityActionAnimArray = selectedProp.name.Equals("anims") && selectedProp.isArray;
                        var isEntitySelector = selectedProp.name.Equals("entity");
                        if (isEntityActionAnimArray)
                        {
                            DrawAnimList(selectedProp);
                        }
                        else if (isEntitySelector)
                        {
                            DrawEntitySelectorDropDown(selectedProp,actualParentObject);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(selectedProp, true);
                            var tcFieldInfo = actualParentObject?.GetType().GetField(
                                selectedProp.name,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (tcFieldInfo?.FieldType == typeof(TransformChecker))
                            {
                                var transformChecker = (TransformChecker)tcFieldInfo.GetValue(actualParentObject);
                                DrawEntityActionTransformCheckerEditingButton(selectedProp,transformChecker);
                            }
                        }

                    }

                } while (selectedProp.NextVisible(false));
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
                    DrawTypeDropdown(valueProp, typeof(EntityAnim));
                }

                GUILayout.FlexibleSpace();

                GUI.backgroundColor = ColorExt.LIGHT_RED;
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
            GUI.backgroundColor = ColorExt.LIGHT_GREEN;
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

        private static void DrawEntityActionTransformCheckerEditingButton(SerializedProperty prop,TransformChecker transformChecker)
        {
            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Edit " + prop.displayName, GUILayout.Height(25)))
            {
                var editingFlowchart = RegisteredFlowchartNodeContext.LinkedData.linkedElement;
                if (editingFlowchart != null && InspectorHelper.InspectorNovaElementWrapper.selectedElement is EntityAction entityAction)
                {
                    var entityPrefabs = editingFlowchart.entityPrefabs;
                    TransformCheckerMono.StartToSetTransform(transformChecker,entityPrefabs[entityAction.entity]?.GetComponent<SpriteRenderer>()?.sprite);
                    TransformCheckerInspectorEditor.ToRestoreElement = entityAction;
                }
            }
            GUI.backgroundColor = Color.white;
                                    
            EditorGUILayout.Space(15);
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
                var condition = attr.Condition;
                try
                {
                    float numValue = Convert.ToSingle(value);
                    float numExpected = Convert.ToSingle(attr.ExpectedValue);
                    return condition switch
                    {
                        ShowInInspectorIfAttribute.ValueCondition.Equals => value.Equals(attr.ExpectedValue),
                        ShowInInspectorIfAttribute.ValueCondition.NoEquals => !value.Equals(attr.ExpectedValue),
                        ShowInInspectorIfAttribute.ValueCondition.MoreThan => numValue > numExpected,
                        ShowInInspectorIfAttribute.ValueCondition.MoreThanOrEqual => numValue >= numExpected,
                        ShowInInspectorIfAttribute.ValueCondition.LessThan => numValue < numExpected,
                        ShowInInspectorIfAttribute.ValueCondition.LessThanOrEqual => numValue <= numExpected,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                catch
                {
                    Debug.Log("?" + value);
                    return true;
                }
            }

            return true;
        }
    }
}
