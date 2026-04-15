using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NovaLine.Script.Utils
{
    #if UNITY_EDITOR
    [ExecuteAlways]
    public class TransformCheckerMono : MonoBehaviour
    {
        public static TransformCheckerMono Instance { get; private set; }

        private TransformChecker _transformChecker;
        public TransformChecker TransformChecker
        {
            get => _transformChecker;
            set
            {
                if (value is RectTransformChecker)
                {
                    if (gameObject.GetComponent<RectTransform>() == null)
                    {
                        gameObject.AddComponent<RectTransform>();
                    }
                }
                _transformChecker = value;
                _transformChecker.LinkedTransform = transform; 
            }
        }
        public bool IsUI => TransformChecker is RectTransformChecker;

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
            
            EditorApplication.delayCall += () => 
            {
                if (this != null && Instance != this)
                {
                    ForceDestroy(gameObject);
                }
            };
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

        public static void StartToSetTransform(TransformChecker transformChecker, MonoBehaviour prefabToDisplay)
        {
            if (Instance != null) Cancel();
            
            Instance = new GameObject($"[Display Only] {prefabToDisplay.name}").AddComponent<TransformCheckerMono>();
            Instance.TransformChecker = transformChecker;
            Instance.transform.SetParent(Instance.IsUI ? NovaPlayer.Instance.UICanvas.transform : NovaPlayer.Instance.transform,false);
            
            MapTransformCheckerComponentAndChildren(Instance.gameObject, prefabToDisplay.gameObject);
            
            Selection.activeObject = Instance.gameObject;
            SceneView.lastActiveSceneView?.Focus();
        }

        public static void SaveTransform()
        {
            if (Instance == null || Application.isPlaying) return;
            Instance.TransformChecker.ImportFromTransform();
            Cancel();
        }

        private static GameObject MapTransformCheckerComponentAndChildren(GameObject to,GameObject from)
        {
            foreach (var component in from.GetComponents<Component>())
            {
                if (IsEssentialTransformCheckerComponent(component))
                {
                    component.CopyComponent(to);
                }
            }

            foreach (Transform childTransform in from.transform)
            {
                var fromChild = childTransform.gameObject;
                var toChild = MapTransformCheckerComponentAndChildren(new GameObject(fromChild.name), fromChild);
                toChild.transform.SetParent(to.transform,false);
            }
            
            return to;
        }
        private static bool IsEssentialTransformCheckerComponent(Component component)
        {
            return component is SpriteRenderer or Image;
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
        [SerializeReference,HideInInspector] protected Transform _linkedTransform;
        public Vector3 position;
        public Vector3 scale = Vector3.one;
        public Quaternion rotation;
        
        public Transform LinkedTransform
        {
            get => _linkedTransform;
            set
            {
                _linkedTransform = value;
                ExportToTransform();
            }
        }
        public virtual void ImportFromTransform()
        {
            if (_linkedTransform == null) return;
            position = _linkedTransform.localPosition;
            scale = _linkedTransform.localScale;
            rotation = _linkedTransform.localRotation;
        }
        public virtual void ExportToTransform()
        {
            if (_linkedTransform == null) return;
            _linkedTransform.localPosition = position;
            _linkedTransform.localScale = scale;
            _linkedTransform.localRotation = rotation;
        }
    }

    [Serializable]
    public class RectTransformChecker : TransformChecker
    {
        public Vector2 pivot = Vector2.one * 0.5f;
        public Vector2 sizeDelta = Vector2.one * 50;
        public Vector2 anchorMin = Vector2.one * 0.5f;
        public Vector2 anchorMax = Vector2.one * 0.5f;
        
        public override void ImportFromTransform()
        {
            base.ImportFromTransform();
            if (_linkedTransform is RectTransform rectTransform)
            {
                pivot = rectTransform.pivot;
                sizeDelta = rectTransform.sizeDelta;
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
            }
        }
        public override void ExportToTransform()
        {
            base.ExportToTransform();
            if (_linkedTransform is RectTransform rectTransform)
            {
                rectTransform.pivot = pivot;
                rectTransform.sizeDelta = sizeDelta;
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
            }
        }
    }
    
    #endif
}