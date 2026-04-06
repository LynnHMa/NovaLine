using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Element;

namespace NovaLine.Script.Utils.Interface
{
    public interface IHasConditionData
    {
        public ConditionData conditionBeforeInvokeData { get; set; }
        public ConditionData conditionAfterInvokeData { get; set; }
    }
}