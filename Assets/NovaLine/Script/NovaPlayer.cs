using System;
using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.UI;
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
        public DialogUI dialogUI;
        
        [Header("Registry")]
        public List<FlowchartDataAsset> flowchartList = new();
        
        [Header("Misc")]
        public Canvas UICanvas;
        public Transform entityStorage;

        private void Awake()
        {
            UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            if(UICanvas.worldCamera == null) UICanvas.worldCamera = Camera.main;
            
            if (!Application.isPlaying) return;
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(playDefault());
        }
        public IEnumerator playDefault()
        {
            for (var i = 0; i < flowchartList.Count; i++)
            {
                var flowchartDataAsset = flowchartList[i];
                yield return PlayFromFlowchart(flowchartDataAsset);
            }
        }

        public static void ResetScene()
        {
            foreach (var entity in EntityRegistry.InstantiatedEntities)
            {
                if(entity?.gameObject == null) continue;
                entity.gameObject.SetActive(false);
            }

            if (Instance?.dialogUI != null)
            {
                Instance.dialogUI.gameObject.SetActive(false);
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
