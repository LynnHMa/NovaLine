using System;
using System.Collections.Generic;
using System.Reflection;
using NovaLine.Script.Action;
using NovaLine.Script.Anim.Entity;
using NovaLine.Script.Data;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.File;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Element;
using NovaLine.Script.Element.Action;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils;
using NovaLine.Script.Utils.Ext;
using NovaLine.Script.Utils.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovaLine.Script.Editor.Utils.InspectorDrawer
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
        private string _currentElementGUID;
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
            _currentElementGUID = wrapper?.selectedElement?.GUID;
            _hasRestoredScroll = false;
            EditorApplication.delayCall += HookInspectorScrollView;
        }

        public override void OnInspectorGUI()
        {
            if (_inspectorScrollView != null && !string.IsNullOrEmpty(_currentElementGUID))
            {
                if (!_hasRestoredScroll)
                {
                    if (_scrollPositionCache.TryGetValue(_currentElementGUID, out Vector2 savedPos))
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
                    _scrollPositionCache[_currentElementGUID] = _inspectorScrollView.scrollOffset;
                }
            }
            serializedObject.Update();

            Draw();

            serializedObject.ApplyModifiedProperties();

            InterceptUndoRedo();
        }

        private void Draw()
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
                            NovaWindow.LoadConditionContextDirect(conditionObj, fallbackName);
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
                            DrawConditionUI("Switch Condition", nodeSwitcher.SwitchCondition, "Switch Condition");
                            EditorGUILayout.Space(10);
                            break;
                    }
                    
                    EditorGUILayout.Space(15);
                }
                
                //Add a button of handling selected node
                if (InspectorHelper.InspectorNovaElementWrapper != null && selectedElement.GUID.Equals(InspectorHelper.InspectorNovaElementWrapper.SelectedElementGUID))
                {
                    var buttonColor = selectedElement.ThemedColor;
                    if (selectedElement is not Flowchart && selectedElement.Type != NovaElementType.Switcher)
                    {
                        EditorGUILayout.Space(10);
                        
                        GUI.backgroundColor = buttonColor;
                        if (GUILayout.Button("Save As", GUILayout.Height(30)))
                        {
                            SaveScope.RequireSave();
                            if (GetContext(selectedElement.GUID,selectedElement.Type)?.LinkedData?.Copy() is IGraphViewNodeData linkedData)
                            {
                                EditorFileManager.SaveAsset(linkedData, null,"Save Asset",selectedElement.name,"Save Asset",true);
                            }
                        }
                        GUI.backgroundColor = Color.white;
                        
                        EditorGUILayout.Space(10);
                            
                        GUI.backgroundColor = buttonColor;
                        if (GUILayout.Button("Import From", GUILayout.Height(30)))
                        {
                            Undo.RecordObject(selectedProp.serializedObject.targetObject, "ImportSave Asset");
                                
                            var openFilePath = EditorUtility.OpenFilePanel("ImportSave Asset",EditorFileManager.CurrentPath,EditorFileManager.GetExtension(selectedElement.Type));
                                
                            if (string.IsNullOrEmpty(openFilePath)) return;
                                    
                            var relativePath = FileUtil.GetProjectRelativePath(openFilePath);
                                
                            if (relativePath == null) return;
                                
                            var openDataAsset = AssetDatabase.LoadAssetAtPath<GraphViewNodeDataAsset>(relativePath);
                                
                            if (openDataAsset == null || !openDataAsset.data.Type.Equals(selectedElement.Type)) return;
                                
                            var currentGraphView = CurrentGraphViewNodeContext.GraphView;
                                
                            var instantiateAndRelinkData = new InstantiatableData(openDataAsset.data.StrongCopy() as IGraphViewNodeData, currentGraphView.MousePos);
                            
                            EditorDataExt.InstantiateDataToReplaceNodeGraphView(instantiateAndRelinkData,selectedElement);
                        }
                        GUI.backgroundColor = Color.white;
                            
                        EditorGUILayout.Space(10);
                        
                        if (selectedElement is not Condition)
                        {
                            GUI.backgroundColor = buttonColor;
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
                        }
                        
                        EditorGUILayout.Space(10);
                    }
                }
                
                //Add a button of play from current node
                if (selectedElement is Node node)
                {
                    GUI.backgroundColor = ColorExt.NODE_THEMED_COLOR;
                    if (GUILayout.Button(EditorApplication.isPlaying ? "Exit" : "Play From Current Node", GUILayout.Height(30)))
                    {
                        var inspectorLinkedElement = InspectorHelper.InspectorNovaElementWrapper.selectedElement;
                        if (!EditorApplication.isPlaying)
                        {
                            EditorNodePlayer.RequestPlayFromNode(node.GUID,inspectorLinkedElement.GUID);
                        }
                        else
                        {
                            EditorApplication.isPlaying = false;
                            EditorApplication.delayCall += () => inspectorLinkedElement.ShowInInspector();
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
                        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(oldElement), newInstance);
                        InspectorHelper.GlobalReplace(oldElement,newElement);
                    }
                    else if (propObj != null)
                    {
                        string beforeJson = JsonUtility.ToJson(propObj);
                        JsonUtility.FromJsonOverwrite(beforeJson, newInstance);
                    }
                    property.managedReferenceValue = newInstance;
                    property.serializedObject.ApplyModifiedProperties();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        private static void DrawEntitySelectorDropDown(SerializedProperty prop,object actualParentObject)
        {
            if (actualParentObject is not EntityAction entityAction || entityAction.Parent?.Parent is not Flowchart flowchart) return;
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
                    
                    if (ShowIfDrawer.ShouldShow(selectedProp, actualParentObject))
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

                            TransformChecker transformChecker = null;
                            if (tcFieldInfo?.FieldType == typeof(TransformChecker))
                            {
                                transformChecker = tcFieldInfo.GetValue(actualParentObject) as TransformChecker;
                            }
                            else if (tcFieldInfo?.FieldType == typeof(RectTransformChecker))
                            {
                                transformChecker = tcFieldInfo.GetValue(actualParentObject) as RectTransformChecker;
                            }
                            if(transformChecker != null) DrawTransformCheckerEditingButton(selectedProp,transformChecker);
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
            GUI.backgroundColor = ColorExt.CONDITION_THEMED_COLOR;
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
        private static void DrawTransformCheckerEditingButton(SerializedProperty prop,TransformChecker transformChecker)
        {
            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Edit " + prop.displayName, GUILayout.Height(25)))
            {
                if(NovaPlayer.Instance == null)
                {
                    Debug.LogError("Can't find NovaPlayer in the scene,please create one!");
                    return;   
                }
                var editingFlowchart = RegisteredFlowchartContext.LinkedData.LinkedElement;
                if (editingFlowchart != null)
                {
                    MonoBehaviour prefab = null;
                    Action<GameObject> additional = null;
                    var selectedElement = InspectorHelper.InspectorNovaElementWrapper.selectedElement;
                    switch (selectedElement)
                    {
                        case EntityAction entityAction:
                            var entityPrefabs = editingFlowchart.entityPrefabs;
                            prefab = entityPrefabs[entityAction.entity];
                            additional = NovaPlayer.InitEntityLayer;
                            break;
                        case BackgroundAction backgroundAction:
                            prefab = NovaPlayer.Instance.background;
                            additional = o =>
                            {
                                o.GetComponent<SpriteRenderer>().sprite = backgroundAction.sprite;
                            };
                            break;
                        case ButtonClickedEvent buttonClickedEvent:
                            prefab = buttonClickedEvent.buttonPrefab;
                            break;
                    }

                    if (prefab != null)
                    {
                        TransformCheckerMono.StartToSetTransform(transformChecker,prefab,additional);
                        TransformCheckerInspectorEditor.ToRestoreElement = selectedElement;
                    }
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
    }
}
