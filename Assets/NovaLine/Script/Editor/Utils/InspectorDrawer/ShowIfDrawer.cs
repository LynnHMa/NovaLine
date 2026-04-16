using System;
using System.Linq;
using System.Reflection;
using NovaLine.Script.Utils.Attribute;
using UnityEditor;
using UnityEngine;

namespace NovaLine.Script.Editor.Utils.InspectorDrawer
{
    [CustomPropertyDrawer(typeof(ShowInInspectorIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property,property.serializedObject.targetObject) ? EditorGUI.GetPropertyHeight(property, label) : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property,property.serializedObject.targetObject))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public static bool ShouldShow(SerializedProperty property, object actualParentObject)
        {
            if (actualParentObject == null) return true;
            
            FieldInfo fieldInfo = actualParentObject.GetType().GetField(
                property.name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo == null) return true;
            
            var attrs = fieldInfo.GetCustomAttributes<ShowInInspectorIfAttribute>();
            var attrList = attrs as ShowInInspectorIfAttribute[] ?? attrs.ToArray();
            
            if (attrList.Length == 0) return true;
            foreach (var attr in attrList)
            {
                if (attr.HideInEditMode && !Application.isPlaying) 
                    return false;
                
                string conditionPath = property.propertyPath.Replace(property.name, attr.ConditionField);
                SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);
                
                bool conditionMet = false;
                
                if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean)
                {
                    conditionMet = conditionProp.boolValue.Equals(attr.ExpectedValue);
                }
                else
                {
                    var conditionField = actualParentObject.GetType().GetField(
                        attr.ConditionField,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (conditionField != null)
                    {
                        var value = conditionField.GetValue(actualParentObject);
                        try
                        {
                            float numValue = Convert.ToSingle(value);
                            float numExpected = Convert.ToSingle(attr.ExpectedValue);
                            
                            conditionMet = attr.Condition switch
                            {
                                ShowInInspectorIfAttribute.ValueCondition.Equals => value.Equals(attr.ExpectedValue),
                                ShowInInspectorIfAttribute.ValueCondition.NoEquals => !value.Equals(attr.ExpectedValue),
                                ShowInInspectorIfAttribute.ValueCondition.MoreThan => numValue > numExpected,
                                ShowInInspectorIfAttribute.ValueCondition.MoreThanOrEqual => numValue >= numExpected,
                                ShowInInspectorIfAttribute.ValueCondition.LessThan => numValue < numExpected,
                                ShowInInspectorIfAttribute.ValueCondition.LessThanOrEqual => numValue <= numExpected,
                                _ => false
                            };
                        }
                        catch
                        {
                            conditionMet = false;
                        }
                    }
                }
                if (!conditionMet) return false; 
            }
            return true;
        }
    }
}