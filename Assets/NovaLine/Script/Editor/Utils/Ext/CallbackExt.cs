using UnityEditor;

namespace NovaLine.Script.Editor.Utils.Ext
{
    public static class CallbackExt
    {
        public static void FrameCall(int frame, System.Action action)
        {
            if (frame <= 0) action?.Invoke();
            else
            {
                EditorApplication.delayCall += () => FrameCall(frame - 1,action);
            }
        }
    }
}