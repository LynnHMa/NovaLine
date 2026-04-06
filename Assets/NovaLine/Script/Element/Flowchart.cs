using System;
using System.Collections;
using UnityEngine;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        public override NovaElementType type => NovaElementType.FLOWCHART;

        public Flowchart()
        {
        }
        public Flowchart(string name)
        {
            this.name = name;
        }
        public override string getTypeName()
        {
            return "[Flowchart]";
        }
        public IEnumerator play()
        {
            if(firstChild is not Node firstNode)
            {
                Debug.Log(666);
                yield break;
            }
            yield return firstNode.run();
            yield return null;
        }
    }
}
