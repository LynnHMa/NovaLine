using System;
using NovaLine.Script.Element;
using NovaLine.Script.Utils;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils.InspectorDrawer
{
    [UnityEditor.CustomEditor(typeof(TransformCheckerMono))]
    public class TransformCheckerInspectorEditor : UnityEditor.Editor
    {
        public static NovaElement ToRestoreElement { get; set; }

        private static Type InspectorWindowType
        {
            get
            {
                _cachedInspectorWindowType ??= typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
                return _cachedInspectorWindowType;
            }
            set =>  _cachedInspectorWindowType = value;
        }
        private static Type _cachedInspectorWindowType;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(15);
            
            GUI.backgroundColor = ColorExt.LIGHT_GREEN;
            if (GUILayout.Button("Save", GUILayout.Height(30)))
            {
                Undo.RecordObject(Selection.activeObject, "Change Transform");
                TransformCheckerMono.SaveTransform();
                RestoreInspectorElement();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = ColorExt.LIGHT_RED;
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                TransformCheckerMono.Cancel();
                RestoreInspectorElement();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RestoreInspectorElement()
        {
            ToRestoreElement?.ShowInInspector();
            var inspectorWindow = EditorWindow.GetWindow(InspectorWindowType);
            inspectorWindow?.Focus();
        }
    }
}