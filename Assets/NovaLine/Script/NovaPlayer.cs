using System.Collections.Generic;
using System.Linq;
using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.Registry;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils.Attribute;
using NovaLine.Script.Utils.Ext;
using UnityEngine;
using static NovaLine.Script.Registry.NovaElementRegistry;

namespace NovaLine.Script
{
    [ExecuteAlways]
    public class NovaPlayer : MonoBehaviour
    {
        public static NovaPlayer Instance { get; private set; }
        public string PlayingNodeGUID { get; set; }
        
        [Header("UI")]
        public Canvas UICanvas;
        public DialogContainerUI dialogContainerUI;
        public ButtonContainerUI buttonContainerUI;
        public SaveMenuContainerUI saveMenuContainerUI;
        
        [Header("Player")]
        public List<GraphViewNodeDataAsset> playList = new();
        
        [Header("Dialog")]
        public bool fadeIn;
        [ShowInInspectorIf(nameof(fadeIn), true)]
        public float fadeInDuration = 0.3f;
        public bool fadeOut;
        [ShowInInspectorIf(nameof(fadeOut), true)]
        public float fadeOutDuration = 0.3f;

        [Header("Layer")] 
        public string defaultEntitySortingLayer = "Default";
        public int defaultEntityOrderLayer = 1;
        public string defaultBackgroundSortingLayer = "Default";
        public int defaultBackgroundOrderLayer = -1;
        
        [Header("Misc")]
        public Transform entityStorage;
        public Background background;

        private void OnValidate()
        {
            UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            if(UICanvas.worldCamera == null) UICanvas.worldCamera = Camera.main;
            
            Instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            //Background sprite renderer initialization
            InitBackgroundLayer();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                ClearElements();
                foreach (var asset in playList)
                {
                    asset.data.RegisterLinkedElement();
                }
                PlayDefault();
            }
        }

        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }

        public void PlayDefault()
        {
            PlayFromAsset(playList.FirstOrDefault());
        }

        public static void InitEntityLayer(GameObject entityObj)
        {
            var sp = entityObj.GetComponent<SpriteRenderer>();
            if (sp == null || Instance == null) return;
            sp.sortingLayerName = Instance.defaultEntitySortingLayer;
            sp.sortingOrder = Instance.defaultEntityOrderLayer;
        }

        public static void InitBackgroundLayer(GameObject backgroundObj = null)
        {
            var actualBackgroundObj = backgroundObj ?? Instance.background.gameObject;
            var sp = actualBackgroundObj.GetComponent<SpriteRenderer>();
            if (sp == null || Instance == null) return;
            sp.sortingLayerName = Instance.defaultBackgroundSortingLayer;
            sp.sortingOrder = Instance.defaultBackgroundOrderLayer;
        }

        public static void ResetScene(Node currentNode)
        {
            foreach (var entity in EntityRegistry.InstantiatedEntities)
            {
                if(entity?.gameObject == null) continue;
                entity.InactiveDebounce();
            }

            if (Instance?.dialogContainerUI != null && !currentNode.ContainsDialogAction())
            {
                DialogContainerUI.Instance.InactiveDebounce();
            }

            if (Instance?.buttonContainerUI != null)
            {
                ButtonRegistry.ClearButtons();
            }

            if (Instance?.background != null)
            {
                Instance.background.SetSpriteDebounce(null);
            }
        }
        
        public static void PlayFromAsset(GraphViewNodeDataAsset playAsset)
        {
            if (playAsset?.data == null) return;
            Instance.StopAllCoroutines();
            switch (FindElement(playAsset.data.GUID))
            {
                case Flowchart flowchart:
                    flowchart.Play().StartCoroutine();
                    break;
                case Node node:
                    node.Run().StartCoroutine();
                    break;
            }
        }
        public static void PlayFromNode(string nodeGUID)
        {
            Instance.StopAllCoroutines();
            if (FindElement(nodeGUID) is Node node)
            {
                node.Run().StartCoroutine();
            }
        }
    }
}
