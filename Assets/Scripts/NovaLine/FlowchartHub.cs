using UnityEngine;
using NovaLine.Element;
using NovaLine.Utils.Ext;

namespace NovaLine
{
    public class FlowchartManager : MonoBehaviour
    {
        public EList<Flowchart> flowcharts { get; set; } = new();
    }
}
