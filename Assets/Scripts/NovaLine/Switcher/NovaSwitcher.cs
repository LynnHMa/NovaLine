using NovaLine.Element;
using NovaLine.Interface;
using System;

namespace NovaLine.Switcher
{
    [Serializable]
    public class NovaSwitcher : INovaSwitcher
    {
        public NovaElement inputElement { get; set; }
        public NovaElement outputElement { get; set; }
        public string guid { get; set; }
        public NovaSwitcher()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
    public interface INovaSwitcher
    {

    }
}
