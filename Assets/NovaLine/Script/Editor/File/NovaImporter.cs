using NovaLine.Script.Data;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace NovaLine.Script.Editor.File
{
    [ScriptedImporter(1, new[] { "nv_flowchart", "nv_node", "nv_action", "nv_condition", "nv_event" })]
    public class NovaImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string jsonContent = System.IO.File.ReadAllText(ctx.assetPath);
            
            var asset = ScriptableObject.CreateInstance<GraphViewNodeDataAsset>();
            Texture2D icon = null;

            if (!string.IsNullOrEmpty(jsonContent))
            {
                EditorJsonUtility.FromJsonOverwrite(jsonContent, asset);
                icon = NovaFileIcon.GetIcon(asset.data.Type);
            }
            
            string fileName = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            ctx.AddObjectToAsset(fileName, asset, icon);
            ctx.SetMainObject(asset);
        }
    }
}