using NovaLine.Script.Data.NodeGraphView;
using NovaLine.Script.Element;

namespace NovaLine.Script.Utils.Interface
{
    public interface IAroundConditionData
    {
        public ConditionData ConditionBeforeInvokeData { get; set; }
        public ConditionData ConditionAfterInvokeData { get; set; }
    }
}