using NovaLine.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NovaLine.Element
{
    [Serializable]
    public class Flowchart : NovaElement
    {
        [HideInInspector]
        public EList<Node> nodes = new();

        [HideInInspector]
        public Node firstNode;

        public Flowchart() { 
            guid = Guid.NewGuid().ToString();
        }
        public Flowchart(string name) : this()
        {
            this.name = name;
        }
        public Flowchart(string name, string describtion, EList<Node> nodes,string guid)
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
        public override string getType()
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
