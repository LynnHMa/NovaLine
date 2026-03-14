using System;
using NovaLine.Element;
using NovaLine.Switcher;
using NovaLine.Utils.Interface;

namespace NovaLine.Editor.Graph.Data.Edge
{
    [Serializable]
    public class EdgeData<PE, EE> : NovaData, IEdgeData
        where PE : NovaElement
        where EE : NovaSwitcher
    {
        public virtual EE linkedSwitcher { get; set; }
        public override string guid => linkedSwitcher?.guid;

        NovaSwitcher IEdgeData.linkedSwitcher { get => linkedSwitcher; set => linkedSwitcher = value as EE; }

        public EdgeData()
        {
        }
        public EdgeData(EE linkedSwitcher)
        {
            this.linkedSwitcher = linkedSwitcher;
        }

        public virtual void onSummon(NovaSwitcher linkedSwitcher)
        {
            this.linkedSwitcher = (EE)linkedSwitcher;
        }
    }
    public interface IEdgeData : IGUID
    {
        void onSummon(NovaSwitcher linkedSwitcher);
        public NovaSwitcher linkedSwitcher { get; set; }
    }
}
