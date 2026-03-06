namespace NovaLine.Editor.Utils
{
    using UnityEditor;
    using UnityEngine;
    using NovaLine.Element;
    using NovaLine.Switcher;

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
            SerializedProperty parentsProp = serializedObject.FindProperty("parentNodes");
            SerializedProperty selectedProp = serializedObject.FindProperty("selectedNodeInfo");

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

                selectedProp.isExpanded = true;

                EditorGUILayout.PropertyField(selectedProp, GUIContent.none, true);
            }
            else if(selectedProp != null && selectedProp.managedReferenceValue is NodeSwitcher selectedSwitcher)
            {
                EditorGUILayout.LabelField("Selected Edge Info", currentStyle);

                selectedProp.isExpanded = true;

                EditorGUILayout.PropertyField(selectedProp, GUIContent.none, true);
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
