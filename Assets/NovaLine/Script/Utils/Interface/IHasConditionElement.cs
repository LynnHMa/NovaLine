using NovaLine.Script.Element;

namespace NovaLine.Script.Utils.Interface
{
    public interface IHasConditionElement
    {
        string conditionBeforeInvokeGuid { get; set; }
        string conditionAfterInvokeGuid { get; set; }

        Condition conditionBeforeInvoke { get; }
        Condition conditionAfterInvoke { get; }
    }
}