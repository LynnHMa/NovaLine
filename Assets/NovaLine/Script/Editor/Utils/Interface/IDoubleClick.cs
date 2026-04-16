using UnityEngine.UIElements;

namespace NovaLine.Script.Editor.Utils.Interface
{
    public interface IDoubleClick
    {
        void OnClick(PointerDownEvent evt);
        void OnDoubleClick(PointerDownEvent evt);
    }
}