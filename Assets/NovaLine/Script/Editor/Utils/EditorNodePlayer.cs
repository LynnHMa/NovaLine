using NovaLine.Script.Editor.Window;
using NovaLine.Script.Registry;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils
{
    public static class EditorNodePlayer
    {
        private const string KEY_IS_PLAYING = "NOVA_IS_PLAYING";
        private const string KEY_NODE_GUID = "NOVA_NODE_TO_PLAY_GUID";
        private const string KEY_INSPECTOR_ELEMENT_GUID = "NOVA_INSPECTOR_ELEMENT_GUID";
        public static void RequestPlayFromNode(string nodeGUID,string inspectorElementGUID)
        {
            SessionState.SetBool(KEY_IS_PLAYING, true);
            SessionState.SetString(KEY_NODE_GUID, nodeGUID);
            SessionState.SetString(KEY_INSPECTOR_ELEMENT_GUID,inspectorElementGUID);
            
            EditorApplication.isPlaying = true;
        }
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (SessionState.GetBool(KEY_IS_PLAYING, false))
                {
                    SessionState.SetBool(KEY_IS_PLAYING, false);
                    string nodeGUID = SessionState.GetString(KEY_NODE_GUID, "");
                    
                    EditorApplication.delayCall += () => 
                    {
                        ExecutePlay(nodeGUID);
                    };
                }
            }
        }
        
        private static void ExecutePlay(string nodeGUID)
        {
            var flowchartData = ContextRegistry.RegisteredFlowchartContext?.LinkedData;
            if (flowchartData != null)
            {
                flowchartData.RegisterLinkedElement();
                
                if (NovaPlayer.Instance != null)
                {
                    NovaPlayer.PlayFromNode(nodeGUID);
                }
                else
                {
                    Debug.LogError("Cant' find nova player instance! Please make sure it existing!");
                }
            }
            
            var inspectorElement = NovaElementRegistry.FindElement(SessionState.GetString(KEY_INSPECTOR_ELEMENT_GUID, nodeGUID));
            inspectorElement?.ShowInInspector();
        }
    }
}