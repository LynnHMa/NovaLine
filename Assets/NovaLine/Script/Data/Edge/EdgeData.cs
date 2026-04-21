using System;
using NovaLine.Script.Element.Switcher;

namespace NovaLine.Script.Data.Edge
{
    [Serializable]
    public class EdgeData<TLinkedSwitcher> : NovaData<TLinkedSwitcher>, IEdgeData
        where TLinkedSwitcher : NovaSwitcher
    {
        NovaSwitcher IEdgeData.LinkedElement
        {
            get => LinkedElement;
            set => LinkedElement = value as TLinkedSwitcher;
        }
        protected EdgeData()
        {
        }
        protected EdgeData(TLinkedSwitcher linkedElement)
        {
            LinkedElement = linkedElement;
        }

        public override void RegisterLinkedElement()
        {
            if (LinkedElement == null) return;
            var toReg = LinkedElement.StrongCopy() as TLinkedSwitcher;
            NovaElementRegistry.RegisterElement(toReg);
        }

        public override void UnregisterLinkedElement()
        {
            if (LinkedElement == null) return;
            NovaElementRegistry.UnregisterElement(LinkedElement.Guid);
        }
        public override void UpdateLinkedElement(bool updateChildren = true)
        {
            LinkedElement = NovaElementRegistry.FindElement(LinkedElement.Guid) as TLinkedSwitcher;
        }
        
        public override INovaData Copy()
        {
            if (base.Copy() is not EdgeData<TLinkedSwitcher> clone) return null;
            clone.LinkedElement = (TLinkedSwitcher)LinkedElement.Copy();
            return clone;
        }
    }
    public interface IEdgeData : INovaData
    {
        new NovaSwitcher LinkedElement { get; set; }
    }
}
