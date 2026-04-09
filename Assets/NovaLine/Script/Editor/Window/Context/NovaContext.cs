using NovaLine.Script.Data;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Editor.Window.Context
{
    public abstract class NovaContext<TLinkedData> : INovaContext where TLinkedData : INovaData
    {
        protected NovaWindow window => NovaWindow.Instance;
        public string guid => linkedData.guid;
        public NovaElementType type => linkedData?.linkedElement?.type ?? NovaElementType.NONE;
        public TLinkedData linkedData { get; set; }
        
        INovaData INovaContext.linkedData { get => linkedData; set => linkedData = (TLinkedData)value; }
        NovaElementType INovaContext.type => type;
        
        protected NovaContext(TLinkedData linkedData)
        {
            this.linkedData = linkedData;
        }

        public abstract void saveData();
    }

    public interface INovaContext : IGUID
    {
        NovaElementType type { get; }
        void saveData();
        INovaData linkedData { get; set; }
    }
}