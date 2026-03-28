using NovaLine.Editor.Utils.Scope;
using NovaLine.Editor.Window;
using NovaLine.Element.Event;

namespace NovaLine.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Action;
    using NovaLine.Editor.File;
    using System;
    using static NovaLine.Editor.Window.WindowContextRegistry;

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
                Debug.LogError("There is a error in drawing elements!" + e.StackTrace);
            }
        }
        private void loadConditionContext(SerializedProperty conditionPropery)
        {
            var targetCondition = conditionPropery.boxedValue as Condition;
            if (targetCondition != null)
            {
                var conditionContext = GetContext(targetCondition.guid, NovaElementType.CONDITION);
                if (conditionContext != null)
                {
                    var actualConditionName = targetCondition.name;

                    if(String.IsNullOrEmpty(actualConditionName))
                    {
                        if (conditionPropery.name.Contains("Before")) actualConditionName = $"Before {targetCondition.parent?.name} Invoke";
                        else if (conditionPropery.name.Contains("After")) actualConditionName = $"After {targetCondition.parent?.name} Invoke";
                        else actualConditionName = "Switch Condition";
                    }

                    targetCondition.name = actualConditionName;
                    NovaWindow.LoadContextInWindow(conditionContext);
                }
            }
        }

        private void drawElement(SerializedProperty selectedProp, GUIStyle style)
        {
            if (selectedProp == null) return;
            var selectedElement = selectedProp.managedReferenceValue as NovaElement;
            if (selectedElement != null)
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
                        if (SerializedProperty.EqualContents(iterator, endProperty))
                            break;

                        if (iterator.name == "conditionBeforeInvoke" || iterator.name == "conditionAfterInvoke" || iterator.name == "switchCondition")
                        {
                            EditorGUILayout.PropertyField(iterator, false);
                            if (GUILayout.Button("Edit", GUILayout.Height(30)))
                            {
                                loadConditionContext(iterator);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator, true);
                        }

                    }
                    while (iterator.NextVisible(false));
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
                        //Debug.Log("【Inspector 拦截】触发自定义 Redo！");
                    }
                    else
                    {
                        CommandRegistry.Undo();
                        //Debug.Log("【Inspector 拦截】触发自定义 Undo！");
                    }
                    
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Y)
                {
                    CommandRegistry.Redo();
                    //Debug.Log("【Inspector 拦截】触发自定义 Redo(Ctrl+Y)！");
                    e.Use();
                }
            }
        }
    }
}
