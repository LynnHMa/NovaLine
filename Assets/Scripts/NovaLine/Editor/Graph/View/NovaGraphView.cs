using System.Collections.Generic;
using System;
using NovaLine.Editor.Graph.Node;
using UnityEngine;

namespace NovaLine.Editor.Graph.View
{
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System;
    using NovaLine.Editor.Graph.Node;
    using NovaLine.Element;

    [Serializable]
    public class NovaGraphView<N,PE,EE> : GraphView, INovaGraphView where N : GraphNode
    {
        public virtual INovaElement root { get; set; }
        public List<N> graphNodes { get; set; } = new();
        protected NovaGraphView(INovaElement root,string name)
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            serializeGraphElements = OnSerializeGraphElements;
            canPasteSerializedData = OnCanPaste;
            unserializeAndPaste = OnUnserializeAndPaste;
            graphViewChanged += OnGraphViewChanged;

            this.root = root;
            this.name = name;
        }
        protected NovaGraphView(INovaElement root, string name,List<N> graphNodes) : this(root,name)
        {
            this.graphNodes = graphNodes;
        }
        protected virtual string getType()
        {
            return "[Default]";
        }
        public virtual string getName()
        {
            return getType() + " " + name;
        }
        public virtual N summonNewGraphNode(Vector2 pos)
        {
            return default;
        }
        public virtual void addGraphNode(N graphNode,bool isInit = false)
        {
            var graphNodeObj = graphNode.targetObject as NovaElement;
            graphNodeObj.parent = root;
            graphNodes?.Add(graphNode);
            AddElement(graphNode);
        }
        public virtual void removeGraphNode(N graphNode)
        {
            graphNodes?.Remove(graphNode);
            RemoveElement(graphNode);
        }
        public virtual N getExistingGraphNode(string guid)
        {
            var elementsList = new List<GraphElement>(graphElements);
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is N node)
                {
                    if (node.guid.Equals(guid)) return node;
                }
            }
            return null;
        }
        protected virtual GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if(change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if(element is N graphNode)
                    {
                        removeGraphNode(graphNode);
                    }
                }
            }
            return change;
        }
        protected virtual string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var elementsList = new List<GraphElement>(elements);
            var copiedData = new CopyPasteData<N>();
            for (int i = 0; i < elementsList.Count; i++)
            {
                var element = elementsList[i];
                if (element is N graphNode)
                {
                    copiedData.data.Add(graphNode);
                    copiedData.pastePos = new Vector2(graphNode.GetPosition().x + 50,graphNode.GetPosition().y + 50); 
                }
            }
            return JsonUtility.ToJson(copiedData);
        }
        protected virtual void OnUnserializeAndPaste(string operationName, string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            var pasteData = JsonUtility.FromJson<CopyPasteData<N>>(data);
            if (pasteData == null || pasteData?.data?.Count == 0) return;

            ClearSelection();

            for (int i = 0; i < pasteData?.data?.Count; i++)
            {
                var nodeData = pasteData?.data?[i];
                if (nodeData is N graphNode)
                {
                    var n = summonNewGraphNode(pasteData.pastePos);
                    addGraphNode(n);
                    AddToSelection(n);
                }
            }
        }
        protected virtual bool OnCanPaste(string data)
        {
            return !string.IsNullOrEmpty(data);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            evt.menu.AppendAction("Add Graph Node", (action) =>
            {
                var e = summonNewGraphNode(graphMousePosition);
                addGraphNode(e);
            });
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            var portsList = new List<Port>(ports);
            for (int i = 0; i < portsList.Count; i++)
            {
                var port = portsList[i];
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            }

            return compatiblePorts;
        }
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);

            if (selectable is N node)
            {
                node.OnSelected();
            }
        }
    }
    public interface INovaGraphView
    {
        public INovaElement root { get; set; }
        string getName();
    }
}
[Serializable]
public class CopyPasteData<T> where T : GraphNode
{
    public List<T> data = new List<T>();
    public Vector2 pastePos;
}