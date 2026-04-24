using System.Collections.Generic;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

namespace NovaLine.Script.Registry
{
    public static class NovaElementRegistry
    {
        public static Dictionary<string, NovaElement> elementDictionary { get; } = new();

        public static bool RegisterElement(NovaElement novaElement)
        {
            return elementDictionary.TryAdd(novaElement.GUID, novaElement);
        }

        public static bool UnregisterElement(string GUID)
        {
            return elementDictionary.Remove(GUID);
        }

        public static NovaElement FindElement(string GUID)
        {
            if (string.IsNullOrEmpty(GUID)) return null;
            elementDictionary.TryGetValue(GUID, out NovaElement result);
            return result;
        }
        
        public static void ReplaceElement(string oldGUID, NovaElement newElement)
        {
            newElement.GUID = oldGUID;
            elementDictionary[oldGUID] = newElement;
            for (var i = 0; i < newElement.ChildrenGUIDList.Count; i++)
            {
                var childGUID = newElement.ChildrenGUIDList[i];
                var childElement = FindElement(childGUID);
                childElement.SetParent(newElement);
            }

            if (newElement is IAroundConditionElement c)
            {
                c.ConditionAfterInvoke?.SetParent(newElement);
                c.ConditionBeforeInvoke?.SetParent(newElement);
            }
        }
        
        public static void ClearElements()
        {
            elementDictionary.Clear();
        }
    }
}