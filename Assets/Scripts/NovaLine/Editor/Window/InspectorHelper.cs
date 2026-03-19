using NovaLine.Editor.Utils;
using NovaLine.Element;
using UnityEditor;

namespace NovaLine.Editor.Window
{
    public static class InspectorHelper
    {
        private static ObjectInspectorWrapper wrapper;
        public static void ShowInInspector(this NovaElement novaElement)
        {
            if (novaElement == null) return;

            var wrapper = ObjectInspectorWrapper.CreateInstance(novaElement);

            Selection.activeObject = wrapper;
        }
    }
}
