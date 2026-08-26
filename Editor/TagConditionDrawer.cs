using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guanomancer.Editor
{
    [CustomPropertyDrawer(typeof(TagCondition<>), true)]
    public class TagConditionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            EnsureManagedReferenceInitialized(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            {
                return lineHeight;
            }

            if (!property.isExpanded)
            {
                return lineHeight;
            }

            // Header line + Operator line + Invert line
            float totalHeight = lineHeight + lineSpacing + lineHeight + lineSpacing + lineHeight;

            SerializedProperty opProp = property.FindPropertyRelative("_operator");
            if (opProp == null) return EditorGUI.GetPropertyHeight(property, label, true);

            var op = (TagConditionOperator)opProp.enumValueIndex;

            switch (op)
            {
                case TagConditionOperator.HasTag:
                case TagConditionOperator.DoesNotHaveTag:
                    totalHeight += lineSpacing + lineHeight;
                    break;

                case TagConditionOperator.ValueEquals:
                case TagConditionOperator.ValueGreaterThan:
                case TagConditionOperator.ValueLessThan:
                case TagConditionOperator.ValueGreaterThanOrEqual:
                case TagConditionOperator.ValueLessThanOrEqual:
                    totalHeight += (lineSpacing + lineHeight) * 2;
                    break;

                case TagConditionOperator.HasAllTags:
                case TagConditionOperator.HasAnyTags:
                case TagConditionOperator.HasNoneOfTags:
                    SerializedProperty tagsProp = property.FindPropertyRelative("_tags");
                    if (tagsProp != null)
                    {
                        totalHeight += lineSpacing + EditorGUI.GetPropertyHeight(tagsProp, true);
                    }
                    totalHeight += lineSpacing + lineHeight;
                    break;

                case TagConditionOperator.AllOf:
                case TagConditionOperator.AnyOf:
                case TagConditionOperator.NoneOf:
                    SerializedProperty subCondsProp = property.FindPropertyRelative("_subConditions");
                    if (subCondsProp != null)
                    {
                        totalHeight += lineSpacing + GetSubConditionsHeight(subCondsProp);
                    }
                    break;
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;

            EnsureManagedReferenceInitialized(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            {
                Rect btnRect = new Rect(position.x, position.y, position.width, lineHeight);
                if (GUI.Button(btnRect, $"Create {label.text} Instance"))
                {
                    InstantiateManagedReference(property);
                }
                EditorGUI.EndProperty();
                return;
            }

            SerializedProperty opProp = property.FindPropertyRelative("_operator");
            SerializedProperty invertProp = property.FindPropertyRelative("_invert");
            SerializedProperty tagProp = property.FindPropertyRelative("_tag");
            SerializedProperty valProp = property.FindPropertyRelative("_value");
            SerializedProperty tagsProp = property.FindPropertyRelative("_tags");
            SerializedProperty threshProp = property.FindPropertyRelative("_threshold");
            SerializedProperty subCondsProp = property.FindPropertyRelative("_subConditions");

            var op = opProp != null ? (TagConditionOperator)opProp.enumValueIndex : TagConditionOperator.HasTag;
            bool invert = invertProp != null && invertProp.boolValue;

            string summary = $"{label.text} [{(invert ? "NOT " : "")}{FormatOperatorName(op)}]";

            Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, summary, true, EditorStyles.foldoutHeader);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect currentRect = new Rect(position.x, position.y + lineHeight + lineSpacing, position.width, lineHeight);

                // 1. Operator Dropdown
                if (opProp != null)
                {
                    EditorGUI.PropertyField(currentRect, opProp, new GUIContent("Condition"));
                    currentRect.y += lineHeight + lineSpacing;
                }

                // 2. Invert Toggle
                if (invertProp != null)
                {
                    EditorGUI.PropertyField(currentRect, invertProp, new GUIContent("Invert Condition"));
                    currentRect.y += lineHeight + lineSpacing;
                }

                // 3. Dynamic Operands based on selected operator
                switch (op)
                {
                    case TagConditionOperator.HasTag:
                    case TagConditionOperator.DoesNotHaveTag:
                        if (tagProp != null)
                        {
                            EditorGUI.PropertyField(currentRect, tagProp, new GUIContent("Tag"));
                            currentRect.y += lineHeight + lineSpacing;
                        }
                        break;

                    case TagConditionOperator.ValueEquals:
                    case TagConditionOperator.ValueGreaterThan:
                    case TagConditionOperator.ValueLessThan:
                    case TagConditionOperator.ValueGreaterThanOrEqual:
                    case TagConditionOperator.ValueLessThanOrEqual:
                        if (tagProp != null)
                        {
                            EditorGUI.PropertyField(currentRect, tagProp, new GUIContent("Tag"));
                            currentRect.y += lineHeight + lineSpacing;
                        }
                        if (valProp != null)
                        {
                            EditorGUI.PropertyField(currentRect, valProp, new GUIContent("Value"));
                            currentRect.y += lineHeight + lineSpacing;
                        }
                        break;

                    case TagConditionOperator.HasAllTags:
                    case TagConditionOperator.HasAnyTags:
                    case TagConditionOperator.HasNoneOfTags:
                        if (tagsProp != null)
                        {
                            float tagsHeight = EditorGUI.GetPropertyHeight(tagsProp, true);
                            Rect tagsRect = new Rect(currentRect.x, currentRect.y, currentRect.width, tagsHeight);
                            EditorGUI.PropertyField(tagsRect, tagsProp, new GUIContent("Tags"), true);
                            currentRect.y += tagsHeight + lineSpacing;
                        }
                        if (threshProp != null)
                        {
                            EditorGUI.PropertyField(currentRect, threshProp, new GUIContent("Threshold"));
                            currentRect.y += lineHeight + lineSpacing;
                        }
                        break;

                    case TagConditionOperator.AllOf:
                    case TagConditionOperator.AnyOf:
                    case TagConditionOperator.NoneOf:
                        if (subCondsProp != null)
                        {
                            DrawSubConditionsList(ref currentRect, subCondsProp, lineSpacing, lineHeight);
                        }
                        break;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private float GetSubConditionsHeight(SerializedProperty subCondsProp)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (!subCondsProp.isExpanded)
            {
                return lineHeight;
            }

            // Header + Size field
            float height = lineHeight + lineSpacing + lineHeight;

            for (int i = 0; i < subCondsProp.arraySize; i++)
            {
                SerializedProperty element = subCondsProp.GetArrayElementAtIndex(i);
                height += lineSpacing + EditorGUI.GetPropertyHeight(element, true);
            }

            return height;
        }

        private void DrawSubConditionsList(ref Rect currentRect, SerializedProperty subCondsProp, float lineSpacing, float lineHeight)
        {
            Rect headerRect = new Rect(currentRect.x, currentRect.y, currentRect.width - 60f, lineHeight);
            Rect buttonsRect = new Rect(currentRect.x + currentRect.width - 55f, currentRect.y, 55f, lineHeight);

            subCondsProp.isExpanded = EditorGUI.Foldout(headerRect, subCondsProp.isExpanded, $"Sub Conditions ({subCondsProp.arraySize})", true, EditorStyles.foldout);

            // [+] [-] buttons in header
            if (GUI.Button(new Rect(buttonsRect.x, buttonsRect.y, 26f, lineHeight), "+", EditorStyles.miniButtonLeft))
            {
                subCondsProp.arraySize++;
                SerializedProperty newElem = subCondsProp.GetArrayElementAtIndex(subCondsProp.arraySize - 1);
                InstantiateManagedReference(newElem);
                subCondsProp.isExpanded = true;
            }

            EditorGUI.BeginDisabledGroup(subCondsProp.arraySize == 0);
            if (GUI.Button(new Rect(buttonsRect.x + 27f, buttonsRect.y, 26f, lineHeight), "-", EditorStyles.miniButtonRight))
            {
                if (subCondsProp.arraySize > 0)
                {
                    subCondsProp.arraySize--;
                }
            }
            EditorGUI.EndDisabledGroup();

            currentRect.y += lineHeight + lineSpacing;

            if (subCondsProp.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Array Size Field
                int newSize = EditorGUI.IntField(new Rect(currentRect.x, currentRect.y, currentRect.width, lineHeight), "Size", subCondsProp.arraySize);
                currentRect.y += lineHeight + lineSpacing;

                if (newSize != subCondsProp.arraySize && newSize >= 0)
                {
                    int oldSize = subCondsProp.arraySize;
                    subCondsProp.arraySize = newSize;
                    for (int i = oldSize; i < newSize; i++)
                    {
                        InstantiateManagedReference(subCondsProp.GetArrayElementAtIndex(i));
                    }
                }

                for (int i = 0; i < subCondsProp.arraySize; i++)
                {
                    SerializedProperty element = subCondsProp.GetArrayElementAtIndex(i);
                    EnsureManagedReferenceInitialized(element);

                    float elemHeight = EditorGUI.GetPropertyHeight(element, true);
                    Rect elemRect = new Rect(currentRect.x, currentRect.y, currentRect.width, elemHeight);

                    EditorGUI.PropertyField(elemRect, element, new GUIContent($"Condition {i + 1}"), true);
                    currentRect.y += elemHeight + lineSpacing;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void EnsureManagedReferenceInitialized(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
            {
                InstantiateManagedReference(property);
            }
        }

        private void InstantiateManagedReference(SerializedProperty property)
        {
            Type targetType = GetTargetType(property);
            if (targetType != null && !targetType.IsAbstract && !targetType.IsInterface)
            {
                try
                {
                    property.managedReferenceValue = Activator.CreateInstance(targetType);
                }
                catch (Exception)
                {
                    // Fallback if parameterless constructor fails
                }
            }
        }

        private Type GetTargetType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (!string.IsNullOrEmpty(typeName))
            {
                string[] parts = typeName.Split(' ');
                if (parts.Length == 2)
                {
                    var foundType = Type.GetType($"{parts[1]}, {parts[0]}");
                    if (foundType != null) return foundType;
                }
            }

            if (fieldInfo != null)
            {
                Type fType = fieldInfo.FieldType;
                if (fType.IsArray) return fType.GetElementType();
                if (fType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(fType.GetGenericTypeDefinition()))
                    return fType.GetGenericArguments()[0];
                return fType;
            }

            return null;
        }

        private static string FormatOperatorName(TagConditionOperator op)
        {
            return op switch
            {
                TagConditionOperator.HasTag => "Has Tag",
                TagConditionOperator.DoesNotHaveTag => "Doesn't Have Tag",
                TagConditionOperator.ValueEquals => "Value ==",
                TagConditionOperator.ValueGreaterThan => "Value >",
                TagConditionOperator.ValueLessThan => "Value <",
                TagConditionOperator.ValueGreaterThanOrEqual => "Value >=",
                TagConditionOperator.ValueLessThanOrEqual => "Value <=",
                TagConditionOperator.HasAllTags => "Has All Tags",
                TagConditionOperator.HasAnyTags => "Has Any Tag",
                TagConditionOperator.HasNoneOfTags => "Has None Of Tags",
                TagConditionOperator.AllOf => "All Of (AND)",
                TagConditionOperator.AnyOf => "Any Of (OR)",
                TagConditionOperator.NoneOf => "None Of (NOR)",
                _ => op.ToString()
            };
        }
    }
}

