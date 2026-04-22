using System;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.File;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using NovaLine.Script.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NovaLine.Script.Editor
{
    public static class NovaMenu
    {
        [MenuItem("NovaLine/File/New Flowchart")]
        public static void NewFlowchartInWindow()
        {
            var newDataAsset = EditorFileManager.CreateNewFlowchartAsset();
            if (newDataAsset == null) return;
            var newFlowchartData = newDataAsset.data as FlowchartData;
            var newContext = new FlowchartContext(newFlowchartData);
            EditorApplication.delayCall += () =>
            {
                EditorFileManager.RegisterAndLoadContext(newContext);
            };
        }

        [MenuItem("NovaLine/Create Player")]
        public static void CreateNovaPlayer()
        {
            try
            {
                var playerPrefab = AssetDatabaseExt.LoadAssetByName<NovaPlayer>("NovaPlayer");
                if (playerPrefab == null)
                {
                    throw(new Exception("The prefab of NovaPlayer not found!"));
                }
                var instantiatedPlayer = Object.Instantiate(playerPrefab);
                instantiatedPlayer.name = "NovaPlayer";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}