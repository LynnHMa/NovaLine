namespace NovaLine.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Switcher;
    using NovaLine.Action;
    using NovaLine.Editor.File;
    using NovaLine.Event;

    [CustomEditor(typeof(ObjectInspectorWrapper))]
    public class ObjectInspectorWrapperEditor : Editor
    {
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
                var parentsProp = serializedObject.FindProperty("parentNodes");
                var selectedProp = serializedObject.FindProperty("selectedNodeInfo");
                var currentStyle = new GUIStyle()
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = ColorExt.light_red },
                    alignment = TextAnchor.MiddleCenter
                };

                var parentStyle = new GUIStyle()
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = ColorExt.orange },
                    alignment = TextAnchor.MiddleCenter
                };

                var arrowStyle = new GUIStyle()
                {
                    fontSize = 35,
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter
                };

                if (parentsProp.arraySize > 0)
                {
                    EditorGUILayout.Space(15);

                    for (int i = 0; i < parentsProp.arraySize; i++)
                    {
                        EditorGUILayout.Space(30);

                        SerializedProperty parentItemProp = parentsProp.GetArrayElementAtIndex(i);

                        var parentElement = parentItemProp.managedReferenceValue as NovaElement;

                        parentItemProp.isExpanded = true;

                        if (parentElement == null) continue;

                        EditorGUILayout.LabelField(parentElement.getActualName(), parentStyle);

                        EditorGUILayout.PropertyField(parentItemProp, GUIContent.none, true);

                        EditorGUILayout.Space(30);
                        EditorGUILayout.LabelField("↓", arrowStyle);
                    }
                }

                EditorGUILayout.Space(30);

                if (selectedProp != null && selectedProp.managedReferenceValue is NovaElement selectedElement)
                {
                    EditorGUILayout.LabelField(selectedElement?.getActualName(), currentStyle);

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
                            EditorGUILayout.Space(30);
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

                    EditorGUILayout.Space(30);

                    if (GUILayout.Button("Set To Start", GUILayout.Height(30)) && NovaWindow.SelectedGraphNode != null)
                    {
                        var currentGraphView = NovaWindow.GetMainWindowInstance()?.currentGraphViewContext?.graphView;
                        if (currentGraphView != null)
                        {
                            currentGraphView.firstNode = NovaWindow.SelectedGraphNode;
                            NovaFileManager.SaveGraphWindowData();
                        }
                    }

                    EditorGUILayout.Space(30);
                }
                else if (selectedProp != null && selectedProp.managedReferenceValue is NodeSwitcher selectedSwitcher)
                {
                    EditorGUILayout.LabelField("Selected Edge Info", currentStyle);

                    selectedProp.isExpanded = true;

                    EditorGUILayout.PropertyField(selectedProp, GUIContent.none, true);
                }
            }
            catch
            {
            }
        }
        private void loadConditionContext(SerializedProperty conditionPropery)
        {
            var targetCondition = conditionPropery.boxedValue as Condition;
            if (targetCondition != null)
            {
                Debug.Log("guid: " + targetCondition.guid);
                var conditionContext = NovaWindow.GetContext(targetCondition.guid);
                if (conditionContext != null)
                {
                    NovaWindow.LoadContextInWindow(conditionContext);
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
