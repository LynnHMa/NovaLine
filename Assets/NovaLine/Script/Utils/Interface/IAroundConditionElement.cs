using NovaLine.Script.Element;

namespace NovaLine.Script.Utils.Interface
{
    public interface IAroundConditionElement
    {
        string ConditionBeforeInvokeGUID { get; set; }
        string ConditionAfterInvokeGUID { get; set; }

        Condition ConditionBeforeInvoke { get; }
        Condition ConditionAfterInvoke { get; }
        void InitConditions();
    }
}