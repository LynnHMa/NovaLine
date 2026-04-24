using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Editor.Window.Context
{
    public abstract class NovaContext<TLinkedData> : INovaContext where TLinkedData : INovaData
    {
        protected NovaWindow Window => NovaWindow.Instance;
        public string GUID => LinkedData.GUID;
        public NovaElementType Type => LinkedData?.LinkedElement?.Type ?? NovaElementType.None;
        public TLinkedData LinkedData { get; set; }
        
        INovaData INovaContext.LinkedData { get => LinkedData; set => LinkedData = (TLinkedData)value; }
        NovaElementType INovaContext.Type => Type;
        
        protected NovaContext(TLinkedData linkedData)
        {
            LinkedData = linkedData;
        }
        
        public virtual void ReplaceLinkedData(INovaData linkedData)
        {
            LinkedData = (TLinkedData)linkedData;
        }

        public abstract void SaveData();
    }

    public interface INovaContext : IGUID
    {
        NovaElementType Type { get; }
        void SaveData();
        INovaData LinkedData { get; set; }
        void ReplaceLinkedData(INovaData linkedData);
    }
}