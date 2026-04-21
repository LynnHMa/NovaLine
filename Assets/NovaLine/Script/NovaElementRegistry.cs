using System.Collections.Generic;
using NovaLine.Script.Element;
using NovaLine.Script.Utils.Interface;

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
            newElement.Guid = oldGuid;
            elementDictionary[oldGuid] = newElement;
            for (var i = 0; i < newElement.ChildrenGuidList.Count; i++)
            {
                var childGuid = newElement.ChildrenGuidList[i];
                var childElement = FindElement(childGuid);
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