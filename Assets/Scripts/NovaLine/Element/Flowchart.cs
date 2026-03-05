using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaLine.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        public List<Node> nodes { get; set; } = new List<Node>();

        public Flowchart() { 
            guid = Guid.NewGuid().ToString();
        }
        public Flowchart(string name) : this()
        {
            this.name = name;
        }
        public Flowchart(string name, string describtion, List<Node> nodes,string guid)
        {
            this.name = name;
            this.describtion = describtion;
            this.nodes = nodes;
            foreach(var node in nodes)
            {
                node.parent = this;
            }
            this.guid = guid;
        }

        public async Task play()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                await nodes[i]?.run();
            }
        }
    }
}
