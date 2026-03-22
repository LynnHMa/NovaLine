using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        public override NovaElementType type => NovaElementType.FLOWCHART;

        [HideInInspector]
        public List<Node> nodes = new();

        [HideInInspector]
        public Node firstNode;

        public Flowchart() { 
            guid = Guid.NewGuid().ToString();
        }
        public Flowchart(string name) : this()
        {
            this.name = name;
        }
        public override string getTypeName()
        {
            return "[Flowchart]";
        }
        public async Task play()
        {
            if(firstNode == null)
            {
                await Task.CompletedTask;
            }
            else
            {
                await firstNode.run();
            }

        }
    }
}
