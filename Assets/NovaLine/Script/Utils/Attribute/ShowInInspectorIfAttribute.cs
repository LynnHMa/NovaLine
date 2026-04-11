namespace NovaLine.Script.Utils.Attribute
{
    using UnityEngine;
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowInInspectorIfAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public object ExpectedValue { get; }
        public bool HideInEditMode { get; }
        
        public ShowInInspectorIfAttribute(string conditionField, object expectedValue, bool hideInEditMode = false)
        {
            ConditionField = conditionField;
            ExpectedValue = expectedValue;
            HideInEditMode = hideInEditMode;
        }
    }
}