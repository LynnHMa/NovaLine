using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System;
using NovaLine.Script.Data;
using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Editor.Utils.Ext;
using NovaLine.Script.Editor.Window.Context.GraphViewNode;
using static NovaLine.Script.Editor.Window.NovaWindow;
using static NovaLine.Script.Editor.Window.ContextRegistry;
using NovaLine.Script.Element;

namespace NovaLine.Script.Editor.File
{
    public static class EditorFileManager
    {
        private const string CURRENT_PATH_SESSION_PATH_KEY = "NOVA_CURRENT_PATH";
        private const string CURRENT_CONTEXT_GUID_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT";
        private const string CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY = "NOVA_CURRENT_CONTEXT_TYPE";

        
        public static string CurrentPath
        {
            get => EditorPrefs.GetString(GetProjectKey(CURRENT_PATH_SESSION_PATH_KEY), string.Empty);
            set => EditorPrefs.SetString(GetProjectKey(CURRENT_PATH_SESSION_PATH_KEY), value);
        }
        public static string CurrentContextGuid
        {
            get => EditorPrefs.GetString(GetProjectKey(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY), string.Empty);
            set => EditorPrefs.SetString(GetProjectKey(CURRENT_CONTEXT_GUID_SESSION_PATH_KEY), value);
        }

        public static NovaElementType CurrentContextType
        {
            get => (NovaElementType) Enum.Parse(typeof(NovaElementType), EditorPrefs.GetString(GetProjectKey(CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY), "None"));
            set => EditorPrefs.SetString(GetProjectKey(CURRENT_CONTEXT_TYPE_SESSION_PATH_KEY), value.ToString());
        }

        private static GraphViewNodeDataAsset _currentAsset;
        public static GraphViewNodeDataAsset CurrentAsset
        {
            get
            {
                if (_currentAsset == null && !string.IsNullOrEmpty(CurrentPath))
                    _currentAsset = AssetDatabase.LoadAssetAtPath<GraphViewNodeDataAsset>(CurrentPath);
                return _currentAsset;
            }
            set => _currentAsset = value;
        }
        [OnOpenAsset]
        public static bool LoadGraphViewNodeDataAsset(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Can't find asset path!");
                return false;
            }
            
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is GraphViewNodeDataAsset asset)
            {
                if (!IsNovaExtension(path)) return false;
                
                if (asset.data == null)
                {
                    Debug.LogError("The asset data is null!");
                    return false;
                }

                if (asset.data.HasGraphView())
                {
                    CurrentAsset = asset;
                    CurrentPath = path;

                    if (CreateContextByType(asset.data, asset.data.LinkedElement.Type) is not IGraphViewNodeContext context) return false;
                    RegisterAndLoadContext(context);
                }
                else Debug.Log("There is no graph view for this type of asset you chosen.");
                return true;
            }
            return false;
        }
        public static void SaveCurrentGraphViewNodeData()
        {
            void BeforeSave(GraphViewNodeDataAsset actionAsset, string actionPath)
            {
                CurrentAsset = actionAsset;
                CurrentPath = actionPath;
                
                if (!CurrentGraphViewNodeContext.Guid.Equals(RootGraphViewNodeContext.Guid))
                {
                    CurrentGraphViewNodeContext.SaveData();
                }
                RootGraphViewNodeContext.SaveData();
                
                EditorUtility.SetDirty(CurrentAsset);
                CurrentAsset.data = RootGraphViewNodeContext.LinkedData;
            }
            
            if (RootGraphViewNodeContext?.LinkedData == null || CurrentGraphViewNodeContext?.LinkedData == null)
                return;
            
            SaveAsset(RootGraphViewNodeContext.LinkedData, BeforeSave, "Save Asset",
                RootGraphViewNodeContext.LinkedData.Name, "Save Asset");
        }

        public static void RestoreAfterDomainReload()
        {
            if (string.IsNullOrEmpty(CurrentPath) || CurrentAsset == null) return;

            var data = CurrentAsset.data;
            if (CreateContextByType(data,data.LinkedElement.Type) is not IGraphViewNodeContext context) return;
            
            //Re-register all contexts
            var storedCurrentContextGuid = CurrentContextGuid;
            var storedCurrentContextType = CurrentContextType;
            
            //Restore root graph view
            RegisterAndLoadContext(context);

            //Restore opened child graph view
            if (!context.Guid.Equals(storedCurrentContextGuid))
            {
                if (GetContext(storedCurrentContextGuid, storedCurrentContextType) is not IGraphViewNodeContext childContext) return;
                LoadContextInWindow(childContext);
            }
        }

        public static void RegisterAndLoadContext(IGraphViewNodeContext context)
        {
            if (context?.LinkedData == null) return;
            
            ClearContexts();
            RegisterContext(context);
            
            NovaElementRegistry.ClearElements();
            context.LinkedData.RegisterLinkedElement();
            
            LoadContextInWindow(context);
        }

        public static GraphViewNodeDataAsset CreateNewFlowchartAsset()
        {
            try
            {
                void BeforeSave(GraphViewNodeDataAsset actionAsset, string actionPath)
                {
                    CurrentAsset = actionAsset;
                    CurrentPath = actionPath;
                }
                return SaveAsset(new FlowchartData(), BeforeSave,"New Flowchart","New Flowchart","New Flowchart",true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        public static GraphViewNodeDataAsset SaveAsset(IGraphViewNodeData data,
            System.Action<GraphViewNodeDataAsset,string> beforeSave,
            string title,string defaultName,string message,bool forceToSaveAs = false)
        {
            var isSaveAs = forceToSaveAs || string.IsNullOrEmpty(CurrentPath);
            
            var path = isSaveAs ? EditorUtility.SaveFilePanelInProject(title, defaultName, GetExtension(data.Type), message) : CurrentPath;
            
            if (string.IsNullOrEmpty(path)) return null;
            
            var asset = AssetDatabase.LoadAssetAtPath<GraphViewNodeDataAsset>(path);
            if (asset == null)
            {
                asset = GraphViewNodeDataAsset.CreateInstance(data);
            }
            
            beforeSave?.Invoke(asset, path);
            
            string json = EditorJsonUtility.ToJson(asset, true);

            System.IO.File.WriteAllText(path, json);
            
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            return asset;
        }

        public static string GetExtension(NovaElementType type)
        {
            return type switch
            {
                NovaElementType.Flowchart => "nv_flowchart",
                NovaElementType.Node => "nv_node",
                NovaElementType.Action => "nv_action",
                NovaElementType.Condition => "nv_condition",
                NovaElementType.Event => "nv_event",
                NovaElementType.Switcher => "nv_switcher",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        private static bool IsNovaExtension(string path)
        {
            return path.EndsWith(".nv_flowchart") || 
                   path.EndsWith(".nv_node") || 
                   path.EndsWith(".nv_action") || 
                   path.EndsWith(".nv_condition") || 
                   path.EndsWith(".nv_event") || 
                   path.EndsWith(".nv_switcher");
        }
        
        private static string GetProjectKey(string key)
        {
            return $"{Application.productName}_{key}";
        }
    }
}
