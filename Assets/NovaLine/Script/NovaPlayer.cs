using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.UI.Container;
using NovaLine.Script.Utils.Attribute;
using UnityEditor;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script
{
    [ExecuteAlways]
    public class NovaPlayer : MonoBehaviour
    {
        public static NovaPlayer Instance { get; private set; }
        
        [Header("UI")]
        public Canvas UICanvas;
        public DialogContainerUI dialogContainerUI;
        public ButtonContainerUI buttonContainerUI;
        
        [Header("Registry")]
        public List<FlowchartDataAsset> flowchartList = new();
        
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

            InitBackgroundLayer();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(PlayDefault());
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public IEnumerator PlayDefault()
        {
            for (var i = 0; i < flowchartList.Count; i++)
            {
                var flowchartDataAsset = flowchartList[i];
                yield return PlayFromFlowchart(flowchartDataAsset);
            }
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
                Instance.StartCoroutine(DialogContainerUI.Instance.HideUI());
            }

            if (Instance?.buttonContainerUI != null)
            {
                ButtonContainerUI.ClearButtons();
            }

            if (Instance?.background != null)
            {
                Instance.background.SetSpriteDebounce(null);
            }
        }
        public static IEnumerator PlayFromFlowchart(FlowchartDataAsset playAsset)
        {
            playAsset.data.RegisterLinkedElement();
            if (FindElement(playAsset.data.Guid) is Flowchart flowchart)
            {
                yield return flowchart.Play();
            }
        }

        public static IEnumerator PlayFromNode(string nodeGuid)
        {
            if (FindElement(nodeGuid) is Node node)
            {
                yield return node.Run();
            }
        }
    }
}
