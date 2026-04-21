using NovaLine.Script.Element;

namespace NovaLine.Script.Utils.Interface
{
    public interface IAroundConditionElement
    {
        string ConditionBeforeInvokeGuid { get; set; }
        string ConditionAfterInvokeGuid { get; set; }

        Condition ConditionBeforeInvoke { get; }
        Condition ConditionAfterInvoke { get; }
        void InitConditions();
    }
}