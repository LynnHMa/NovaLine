using System.Collections.Generic;
using NovaLine.Script.Editor.Utils.Ext;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NovaLine.Script.Editor.Window
{
    public static class CopyPasteHelper
    {
        public static string Copy(IEnumerable<GraphElement> elements)
        {
            var copiedData = new InstantiatableData(elements);
            return JsonUtility.ToJson(copiedData);
        }

        public static void Paste(string operationName, string data)
        {
            if (!OnCanPaste(data)) return;
            var copiedData = JsonUtility.FromJson<InstantiatableData>(data);
            if (copiedData != null)
            {
                EditorDataExt.InstantiateDataIntoCurrentGraphView(copiedData);
            }
        }
        
        public static bool OnCanPaste(string data)
        {
            return !string.IsNullOrEmpty(data);
        }
    }
}