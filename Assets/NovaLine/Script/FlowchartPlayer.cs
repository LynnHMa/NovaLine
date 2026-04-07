using System.Collections;
using System.Collections.Generic;
using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.UI;
using UnityEngine;

namespace NovaLine.Script
{
    public class FlowchartPlayer : MonoBehaviour
    {
        public static FlowchartPlayer Instance { get; private set; }
        
        public DialogUI dialogUI;
        public List<FlowchartDataAsset> playList = new();
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(playDefault());
        }

        private IEnumerator playDefault()
        {
            for (var i = 0; i < playList.Count; i++)
            {
                var flowchartDataAsset = playList[i];
                yield return playSingle(flowchartDataAsset);
            }
        }

        private IEnumerator playSingle(FlowchartDataAsset playAsset)
        {
            playAsset.data.registerLinkedElement();
            if (NovaElementRegistry.FindElement(playAsset.data.guid) is Flowchart flowchart)
            {
                yield return flowchart.play();
            }
        }
    }
}
