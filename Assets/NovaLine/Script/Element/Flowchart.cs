using System;
using System.Collections;
using System.Collections.Generic;

namespace NovaLine.Script.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        public override NovaElementType type => NovaElementType.FLOWCHART;
        public List<Entity> entityPrefabs;

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
            if(firstChild is Node firstNode)
            {
                yield return firstNode.run();
            }

            yield return null;
        }
    }
}
