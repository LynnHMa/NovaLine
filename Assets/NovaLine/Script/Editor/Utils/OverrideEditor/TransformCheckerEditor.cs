using NovaLine.Script.Element;
using NovaLine.Script.Utils;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils.OverrideEditor
{
    [UnityEditor.CustomEditor(typeof(TransformCheckerMono))]
    public class TransformCheckerEditor : UnityEditor.Editor
    {
        public static NovaElement ToRestoreElement { get; set; }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(15);
            
            GUI.backgroundColor = ColorExt.LIGHT_GREEN;
            if (GUILayout.Button("Save", GUILayout.Height(30)))
            {
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
        }
    }
}