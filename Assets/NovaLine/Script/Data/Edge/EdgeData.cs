using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class EdgeData<TLinkedSwitcher> : NovaData<TLinkedSwitcher>, IEdgeData
        where TLinkedSwitcher : NovaSwitcher
    {
        NovaSwitcher IEdgeData.linkedElement
        {
            get => linkedElement;
            set => linkedElement = value as TLinkedSwitcher;
        }
        protected EdgeData()
        {
        }
        protected EdgeData(TLinkedSwitcher linkedElement)
        {
            this.linkedElement = linkedElement;
        }

        public override void registerLinkedElement()
        {
            if (linkedElement == null) return;
            var toReg = linkedElement.strongCopy() as TLinkedSwitcher;
            NovaElementRegistry.RegisterElement(toReg);
        }
        public override void updateLinkedElement(bool updateChildren = true)
        {
            linkedElement = NovaElementRegistry.FindElement(linkedElement.guid) as TLinkedSwitcher;
        }
        
        public override INovaData copy()
        {
            if (base.copy() is not EdgeData<TLinkedSwitcher> clone) return null;
            clone.linkedElement = (TLinkedSwitcher)linkedElement.copy();
            return clone;
        }
    }
    public interface IEdgeData : INovaData
    {
        new NovaSwitcher linkedElement { get; set; }
    }
}
