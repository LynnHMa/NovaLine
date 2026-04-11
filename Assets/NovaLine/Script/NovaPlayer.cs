using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.UI;
using UnityEngine;
using static NovaLine.Script.NovaElementRegistry;

namespace NovaLine.Script
{
    public class NovaPlayer : MonoBehaviour
    {
        public static NovaPlayer Instance { get; private set; }
        
        [Header("UI")]
        public DialogUI dialogUI;
        
        [Header("Registry")]
        public List<FlowchartDataAsset> flowchartList = new();
        
        [Header("Misc")]
        public Transform entityStorage;
        private void Awake()
        {
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
                yield return playFromFlowchart(flowchartDataAsset);
            }
        }

        public static IEnumerator playFromFlowchart(FlowchartDataAsset playAsset)
        {
            playAsset.data.registerLinkedElement();
            if (FindElement(playAsset.data.guid) is Flowchart flowchart)
            {
                yield return flowchart.play();
            }
        }

        public static IEnumerator playFromNode(Node node)
        {
            //todo register linked data element
            yield return node.run();
        }
    }
}
