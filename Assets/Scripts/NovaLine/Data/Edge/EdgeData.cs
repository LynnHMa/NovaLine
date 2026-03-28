using System;
using NovaLine.Element.Switcher;
using NovaLine.Utils.Interface;
using UnityEngine;

namespace NovaLine.Data.Edge
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

        public virtual void onSummon(NovaSwitcher linkedSwitcher)
        {
            this.linkedSwitcher = (EE)linkedSwitcher;
        }

        public override INovaData copy()
        {
            var clone = base.copy() as EdgeData<EE>;
            if (clone == null) return null;
            clone.linkedSwitcher = (EE)linkedSwitcher.copy();
            return clone;
        }
    }
    public interface IEdgeData : INovaData,IGUID
    {
        void onSummon(NovaSwitcher linkedSwitcher);
        public NovaSwitcher linkedSwitcher { get; set; }
    }
}
