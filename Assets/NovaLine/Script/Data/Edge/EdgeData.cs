using System;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Utils.Interface;
using UnityEngine;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class EdgeData<EE> : NovaData, IEdgeData
        where EE : NovaSwitcher
    {
        [SerializeReference] private EE _linkedSwitcher;
        
        public virtual EE linkedSwitcher { get => _linkedSwitcher; set =>  _linkedSwitcher = value; }
        public override string guid => linkedSwitcher?.guid;

        NovaSwitcher IEdgeData.linkedSwitcher { get => linkedSwitcher; set => linkedSwitcher = value as EE; }

        protected EdgeData()
        {
        }
        protected EdgeData(EE linkedSwitcher)
        {
            this.linkedSwitcher = linkedSwitcher;
        }

        public virtual void init(NovaSwitcher linkedSwitcher)
        {
            this.linkedSwitcher = (EE)linkedSwitcher;
        }

        public virtual void registerLinkedElement()
        {
            var toReg = linkedSwitcher.strongCopy() as EE;
            NovaElementRegistry.RegisterElement(toReg);
        }
        public virtual void updateLinkedElement()
        {
            linkedSwitcher = NovaElementRegistry.FindElement(linkedSwitcher.guid) as EE;
        }
        public override INovaData copy()
        {
            if (base.copy() is not EdgeData<EE> clone) return null;
            clone.linkedSwitcher = (EE)linkedSwitcher.copy();
            return clone;
        }
    }
    public interface IEdgeData : INovaData,IGUID
    {
        public NovaSwitcher linkedSwitcher { get; set; }
        void init(NovaSwitcher linkedSwitcher);
        void registerLinkedElement();
        void updateLinkedElement();
    }
}
