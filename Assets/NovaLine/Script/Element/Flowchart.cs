using System;
using System.Collections;
using System.Collections.Generic;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        public List<Entity> entityPrefabs;
        
        public override NovaElementType Type => NovaElementType.FLOWCHART;

        public Flowchart()
        {
        }
        public Flowchart(string name)
        {
            this.name = name;
        }
        public override string GetTypeName()
        {
            return "[Flowchart]";
        }
        public IEnumerator Play()
        {
            if(FirstChild is Node firstNode)
            {
                yield return firstNode.Run();
            }

            yield return null;
        }
    }
}
