using System;
using NovaLine.Script.Utils.Ext;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NovaLine.Script.Utils
{
    #if UNITY_EDITOR
    [ExecuteAlways]
    public class TransformCheckerMono : MonoBehaviour
    {
        private const string INSTANCE_NAME = "NovaLine Transform Editor";
        public static TransformCheckerMono Instance { get; private set; }
        public TransformChecker TransformChecker { get; set; }

        private void OnEnable()
        {
            EditorApplication.update += HijackFocus;
        }

        private void OnDisable()
        {
            EditorApplication.update -= HijackFocus;
        }
        private void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(this)) return;
            
            var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (prefabStage != null) return;
            
            if (Instance != this)
            {
                EditorApplication.delayCall += () => 
                {
                    if (this != null)
                    {
                        ForceDestroy(gameObject);
                    }
                };
            }
        }
        
        private void HijackFocus()
        {
            if (Instance == null) return;
            
            if (EditorWindow.focusedWindow?.GetType().Name == "InspectorWindow")
            {
                SceneView.lastActiveSceneView?.Focus();
            }
            
            if (Instance == this && Selection.activeObject != gameObject)
            {
                Selection.activeObject = gameObject;
            }
        }

        public static void StartToSetTransform(TransformChecker checker, Sprite sprite)
        {
            if (Instance != null) Cancel();
            
            var prefab = Resources.Load<TransformCheckerMono>($"Prefab/TransformChecker");
            Instance = prefab.Instantiate(checker.position, checker.scale,checker.rotation);
            Instance.name = INSTANCE_NAME;
            Instance.GetComponent<SpriteRenderer>().sprite = sprite;
            Instance.TransformChecker = checker;
            
            Selection.activeObject = Instance.gameObject;
            SceneView.lastActiveSceneView?.Focus();
        }

        public static void SaveTransform()
        {
            if (Instance == null || Application.isPlaying) return;
            Instance.TransformChecker.position = Instance.transform.localPosition;
            Instance.TransformChecker.scale = Instance.transform.localScale;
            Instance.TransformChecker.rotation = Instance.transform.localRotation;
            Cancel();
        }

        public static void Cancel()
        {
            ForceDestroy(Instance?.gameObject);
            Instance = null;
        }

        private static void ForceDestroy(Object obj)
        {
            if (obj == null || PrefabUtility.IsPartOfPrefabAsset(obj)) return;
            DestroyImmediate(obj);
        }
    }
    
    [Serializable]
    public class TransformChecker
    {        
        public Vector3 position;
        public Vector3 scale = Vector3.one;
        public Quaternion rotation;
    }
    #endif
}