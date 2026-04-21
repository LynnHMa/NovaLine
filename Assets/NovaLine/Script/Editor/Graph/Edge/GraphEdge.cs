using NovaLine.Script.Editor.Utils;
using NovaLine.Script.Editor.Utils.Interface;
using NovaLine.Script.Element.Switcher;
using NovaLine.Script.Editor.Graph.Port;
using NovaLine.Script.Element;
using UnityEngine;
using UnityEngine.UIElements;
using NovaLine.Script.Utils.Interface;
using static NovaLine.Script.Editor.Window.ContextRegistry;

namespace NovaLine.Script.Editor.Graph.Edge
{
    public class GraphEdge<PE, EE> : UnityEditor.Experimental.GraphView.Edge, IGraphEdge, IDoubleClick where PE : NovaElement where EE : NovaSwitcher
    {
        private const float ARROW_WIDTH = 32f;
        private const float ARROW_HEIGHT_HALF = 8f;
        protected virtual Color ThemedColor => Color.green;
        public virtual EE LinkedElement { get; set; }
        public virtual string Guid => LinkedElement?.Guid;
        private readonly VisualElement _arrowElement;

        NovaSwitcher IGraphEdge.LinkedElement { get => LinkedElement; set => LinkedElement = value as EE; }
        string IGUID.Guid => Guid;
        public GraphPort<PE, EE> Input
        {
            get => input as GraphPort<PE, EE>;
            set
            {
                if (input != value)
                {
                    input = value;
                }
            }
        }

        public GraphPort<PE, EE> Output
        {
            get => output as GraphPort<PE, EE>;
            set
            {
                if (output != value)
                {
                    output = value;
                }
            }
        }

        public GraphEdge()
        {
            _arrowElement = new VisualElement
            {
                style =
                {
                    width = 0,
                    height = 0,
                    borderTopWidth = ARROW_HEIGHT_HALF,
                    borderBottomWidth = ARROW_HEIGHT_HALF,
                    borderLeftWidth = ARROW_WIDTH,
                    borderRightWidth = 0,
                    borderTopColor = new StyleColor(Color.clear),
                    borderBottomColor = new StyleColor(Color.clear),
                    borderLeftColor = new StyleColor(ThemedColor),
                    position = Position.Absolute
                },
                pickingMode = PickingMode.Ignore
            };

            Add(_arrowElement);
            
            RegisterCallback<PointerDownEvent>(OnClick);
        }

        public override bool UpdateEdgeControl()
        {
            var result = base.UpdateEdgeControl();
            UpdateArrow();
            return result;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (LinkedElement == null) return;

            LinkedElement.ShowInInspector();
        }
        public override void OnUnselected()
        {
            base.OnUnselected();

            var activeRoot = (NovaElement)CurrentGraphViewNodeContext?.GraphView?.LinkedElement;

            if (activeRoot == null) return;

            activeRoot.ShowInInspector();
        }

        public virtual void UpdateArrow()
        {
            if (edgeControl?.controlPoints == null || edgeControl.controlPoints.Length < 4) return;

            var p0 = edgeControl.controlPoints[0];
            var p1 = edgeControl.controlPoints[1];
            var p2 = edgeControl.controlPoints[2];
            var p3 = edgeControl.controlPoints[3];

            const float t = 0.5f;
            var position = GetBezierPoint(t, p0, p1, p2, p3);
            var tangent = GetBezierTangent(t, p0, p1, p2, p3);
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            _arrowElement.style.left = position.x - (ARROW_WIDTH / 2f);
            _arrowElement.style.top = position.y - ARROW_HEIGHT_HALF;

            _arrowElement.transform.rotation = Quaternion.Euler(0, 0, angle);
            _arrowElement.style.borderLeftColor = new StyleColor(ThemedColor);
        }

        public virtual EE GenerateNewLinkedElement()
        {
            return default;
        }
        private static Vector2 GetBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
            return p;
        }
        private static Vector2 GetBezierTangent(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            Vector2 tangent = 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
            return tangent.normalized;
        }

        public void OnClick(PointerDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                OnDoubleClick(evt);
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }

        public virtual void OnDoubleClick(PointerDownEvent evt){}
    }
    public interface IGraphEdge : IGUID
    {
        public NovaSwitcher LinkedElement { get; set; }
    }
}
