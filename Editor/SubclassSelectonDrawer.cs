using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Guanomancer.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute)), CanEditMultipleObjects]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, label, true);

            Rect buttonRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );

            string currentTypeName = property.managedReferenceValue?.GetType().Name ?? "(None)";

            if (GUI.Button(buttonRect, currentTypeName, EditorStyles.popup))
            {
                ShowMenu(property);
            }

            EditorGUI.EndProperty();
        }

        private void ShowMenu(SerializedProperty property)
        {
            Type baseType = GetBaseType(property);
            if (baseType == null) return;

            var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => t.IsClass && !t.IsAbstract && !typeof(UnityEngine.Object).IsAssignableFrom(t));

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("(None)"), property.managedReferenceValue == null, () => AssignType(property, null));
            menu.AddSeparator("");

            foreach (var type in derivedTypes)
            {
                bool isSelected = property.managedReferenceValue?.GetType() == type;
                menu.AddItem(new GUIContent(type.Name), isSelected, () => AssignType(property, type));
            }

            menu.ShowAsContext();
        }

        private void AssignType(SerializedProperty property, Type type)
        {
            property.serializedObject.Update();

            property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);

            property.serializedObject.ApplyModifiedProperties();
        }

        private Type GetBaseType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            string[] parts = typeName.Split(' ');
            if (parts.Length != 2) return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == parts[0]);
            return assembly?.GetType(parts[1]);
        }
    }
}