using System.Collections.Generic;
using NovaLine.Script.Element;

namespace NovaLine.Script
{
    public static class NovaElementRegistry
    {
        public static Dictionary<string, NovaElement> elementDictionary { get; } = new();

        public static bool RegisterElement(NovaElement novaElement)
        {
            return elementDictionary.TryAdd(novaElement.Guid, novaElement);
        }

        public static bool UnregisterElement(string guid)
        {
            return elementDictionary.Remove(guid);
        }

        public static NovaElement FindElement(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            elementDictionary.TryGetValue(guid, out NovaElement result);
            return result;
        }
        
        public static void ReplaceElement(string oldGuid, NovaElement newElement)
        {
            if (!string.IsNullOrEmpty(oldGuid))
                elementDictionary.Remove(oldGuid);

            if (newElement != null)
                elementDictionary[newElement.Guid] = newElement;
        }
        
        public static void Clear()
        {
            elementDictionary.Clear();
        }
    }
}