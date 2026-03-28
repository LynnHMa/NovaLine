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
            if(firstChild == null)
            {
                await Task.CompletedTask;
            }
            else
            {
                var firstNode = firstChild as Node;
                if (firstNode == null)
                {
                    await Task.CompletedTask;
                    return;
                }
                await firstNode.run();
            }

        }
    }
}
