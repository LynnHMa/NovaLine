using NovaLine.Script.Editor.Utils.Scope;
using NovaLine.Script.Editor.Window;
using NovaLine.Script.Element.Event;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Script.Element;
    using NovaLine.Script.Action;
    using NovaLine.Script.Editor.File;
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

            modifyInfo();

            serializedObject.ApplyModifiedProperties();
            
            interceptUndoRedo();
        }
        private void modifyInfo()
        {
            try
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

                        drawElement(parentItemProp, SELECTED_PARENT_ELEMENT_STYLE);

                        EditorGUILayout.LabelField("↓", ARROW_STYLE);
                    }
                }
                //Draw selected element
                drawElement(selectedProp, SELECTED_ELEMENT_STYLE);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
        private void loadConditionContextDirect(Condition targetCondition, string fallbackName)
        {
            if (targetCondition == null)
            {
                Debug.LogWarning("Can't load condition context ,because Condition is null!");
                return;
            }

            var conditionContext = GetContext(targetCondition.guid, NovaElementType.CONDITION);
            if (conditionContext != null)
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

        private void drawElement(SerializedProperty selectedProp, GUIStyle style)
        {
            if (selectedProp == null) return;
            if (selectedProp.managedReferenceValue is NovaElement selectedElement)
            {
                EditorGUILayout.Space(30);

                EditorGUILayout.LabelField(selectedElement.getActualName(), style);
                
                if (selectedElement is NovaAction)
                {
                    SerializeReferenceUI.DrawTypeDropdown(selectedProp, typeof(INovaAction), "Action Type");
                }
                else if (selectedElement is NovaEvent)
                {
                    SerializeReferenceUI.DrawTypeDropdown(selectedProp, typeof(INovaEvent), "Event Type");
                }

                selectedProp.isExpanded = true;

                SerializedProperty iterator = selectedProp.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();

                if (iterator.NextVisible(true))
                {
                    do
                    {
                        EditorGUILayout.Space(10);
                        if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    while (iterator.NextVisible(false));
                }
                
                EditorGUILayout.Space(15);
                if (selectedElement is IHasConditionElement or NodeSwitcher)
                {
                    void DrawConditionUI(string label, Condition conditionObj, string fallbackName)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false; 
                        string displayName = conditionObj != null ? conditionObj.getActualName() : "Null / 未分配";
                        EditorGUILayout.TextField(displayName);
                        GUI.enabled = true;
                        if (GUILayout.Button("Edit", GUILayout.Width(60), GUILayout.Height(20)))
                        {
                            loadConditionContextDirect(conditionObj, fallbackName);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    if (selectedElement is IHasConditionElement conditionElement)
                    {
                        DrawConditionUI("Condition Before Invoke", conditionElement.conditionBeforeInvoke, $"Before Invoke");
                        EditorGUILayout.Space(10);
                        DrawConditionUI("Condition After Invoke", conditionElement.conditionAfterInvoke, $"After Invoke");
                    }
                    else if (selectedElement is NodeSwitcher nodeSwitcher)
                    {
                        DrawConditionUI("Switch Condition", nodeSwitcher.switchCondition, "Switch Condition");
                        EditorGUILayout.Space(10);
                    }
                }
                
                if (NovaWindow.SelectedGraphNode != null && NovaWindow.SelectedGraphNode.linkedElement.guid.Equals(selectedElement.guid) && NovaWindow.SelectedGraphNode.inputContainer.childCount != 0 && NovaWindow.SelectedGraphNode.outputContainer.childCount != 0)
                {
                    EditorGUILayout.Space(30);
                    if (GUILayout.Button("Set To Start", GUILayout.Height(30)))
                    {
                        var currentGraphView = CurrentGraphViewContext?.graphView;
                        if (currentGraphView != null)
                        {
                            currentGraphView.setFirstNode(NovaWindow.SelectedGraphNode);
                            currentGraphView.update();
                            SaveScope.RequireSave();
                        }
                    }
                    EditorGUILayout.Space(30);
                }

            }
        }

        private void interceptUndoRedo()
        {
            Event e = Event.current;
            
            if (e.type == EventType.KeyDown && e.isKey)
            {
                if (EditorGUIUtility.editingTextField)
                {
                    return;
                }
                if (e.keyCode == KeyCode.Z)
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
                }
                else if (e.keyCode == KeyCode.Y)
                {
                    CommandRegistry.Redo();
                    e.Use();
                }
            }
        }
    }
}
