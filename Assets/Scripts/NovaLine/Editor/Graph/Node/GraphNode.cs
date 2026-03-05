using System;
using UnityEditor;
using UnityEngine;
using NovaLine.Utils;
using UnityEngine.UIElements;
using NovaLine.Element;

namespace NovaLine.Editor.Graph.Node
{
    [Serializable]
    public class GraphNode : UnityEditor.Experimental.GraphView.Node
    {
        public object targetObject { get; set; }

        public string guid { get; set; }

        public override string title => getType() + " " + base.title;

        public virtual Vector2 pos => new Vector2(base.GetPosition().x, base.GetPosition().y);

        private ObjectInspectorWrapper wrapper;

        public GraphNode()
        {
            guid = Guid.NewGuid().ToString();
            RegisterCallback<MouseDownEvent>(onDoubleClick);
        }
        public GraphNode(NovaElement element, Vector2 pos) : this()
        {
            SetPosition(new Rect(pos.x, pos.y, 200, 150));
            base.title = element.name;
            targetObject = element;
            guid = element.guid;
            addPort(element);
        }
        public void SetPosition(Vector2 pos)
        {
            base.SetPosition(new Rect(pos.x,pos.y,base.GetPosition().width,base.GetPosition().height));
        }
        public override void OnSelected()
        {
            base.OnSelected();

            if (targetObject == null) return;

            if (wrapper == null)
            {
                wrapper = ScriptableObject.CreateInstance<ObjectInspectorWrapper>();

                wrapper.hideFlags = HideFlags.DontSave;

                wrapper.name = getType();
            }
            wrapper.objectData = targetObject;

            Selection.activeObject = wrapper;
        }
        protected virtual void onDoubleClick(MouseDownEvent evt)
        {
        }
        public virtual string getType()
        {
            return "[Default]";
        }
        protected virtual void addPort(INovaElement element)
        {
        }
    }
}