namespace NovaLine.Script.Utils.Attribute
{
    using UnityEngine;
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowInInspectorIfAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public object ExpectedValue { get; }
        public ValueCondition Condition { get; }
        public bool HideInEditMode { get; }
        
        public ShowInInspectorIfAttribute(string conditionField, object expectedValue, ValueCondition condition = ValueCondition.Equals,bool hideInEditMode = false)
        {
            ConditionField = conditionField;
            ExpectedValue = expectedValue;
            Condition = condition;
            HideInEditMode = hideInEditMode;
        }

        public enum ValueCondition
        {
            Equals,
            NoEquals,
            MoreThan,
            MoreThanOrEqual,
            LessThan,
            LessThanOrEqual,
        }
    }
}