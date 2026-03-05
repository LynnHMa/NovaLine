
namespace NovaLine.Editor.Graph.Edge
{
    using System;
    using UnityEditor.Experimental.GraphView;
    using NovaLine.Editor.Graph.Port;
    using NovaLine.Element;
    using NovaLine.Switcher;

    public class GraphEdge<PE,EE> : Edge where PE : NovaElement where EE : NovaSwitcher
    {
        public string guid { get; set; }

        public EE linkedElement { get; set; }

        public virtual event Action<GraphEdge<PE,EE>, bool> OnPortConnectionChanged;

        public virtual void generateNewLinkedElement()
        {
        }

        public new GraphPort<PE, EE> input
        {
            get => base.input as GraphPort<PE, EE>;
            set
            {
                if (base.input != value)
                {
                    base.input = value;
                    OnPortConnectionChanged?.Invoke(this, true);
                }
            }
        }

        public new GraphPort<PE, EE> output
        {
            get => base.output as GraphPort<PE, EE>;
            set
            {
                if (base.output != value)
                {
                    base.output = value;
                    OnPortConnectionChanged?.Invoke(this, false);
                }
            }
        }
    }
}
