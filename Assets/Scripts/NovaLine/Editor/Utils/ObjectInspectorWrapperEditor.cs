namespace NovaLine.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Action;
    using NovaLine.Editor.File;
    using NovaLine.Event;
    using System;

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
                var conditionContext = NovaWindow.GetContext(targetCondition.guid,Window.Context.ContextType.CONDITION);
                conditionContext.linkedData.linkedElement.name = conditionPropery.name.Contains("Before") ? "Before" : "After";
                if (conditionContext != null)
                {
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

                // 绘制多态下拉框
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

                        if (iterator.name == "conditionBeforeInvoke" || iterator.name == "conditionAfterInvoke")
                        {
                            EditorGUILayout.PropertyField(iterator, false);
                            if (GUILayout.Button("Edit", GUILayout.Height(30)))
                            {
                                loadConditionContext(iterator);
                            }
                        }
                        else
                        {
                            // 默认画出来
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
                        var currentGraphView = NovaWindow.GetMainWindowInstance()?.currentGraphViewContext?.graphView;
                        if (currentGraphView != null)
                        {
                            currentGraphView.firstNode = NovaWindow.SelectedGraphNode;
                            currentGraphView.update();
                            EditorFileManager.SaveGraphWindowData();
                        }
                    }
                    EditorGUILayout.Space(30);
                }

            }
        }

        private void DrawUILine(Color color, int thickness = 1, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }
}
