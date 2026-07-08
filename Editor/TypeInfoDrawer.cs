using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Guanomancer.Editor
{
    [CustomPropertyDrawer(typeof(TypeInfo<>)), CanEditMultipleObjects]
    public class TypeInfoDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, Type[]> _typesCache = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, GUIContent[]> _namesCache = new Dictionary<Type, GUIContent[]>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            Type targetInterface = fieldType.GetGenericArguments()[0];

            if (!_typesCache.ContainsKey(targetInterface))
            {
                var foundTypes = TypeCache.GetTypesDerivedFrom(targetInterface)
                    .Where(t => /*t.IsValueType &&*/ !t.IsAbstract)
                    .ToList();

                foundTypes.Insert(0, null);

                _typesCache[targetInterface] = foundTypes.ToArray();
                _namesCache[targetInterface] = foundTypes
                    .Select(t => new GUIContent(t == null ? "(None)" : t.Name))
                    .ToArray();
            }

            Type[] types = _typesCache[targetInterface];
            GUIContent[] typeNames = _namesCache[targetInterface];

            SerializedProperty nameProp = property.FindPropertyRelative("_assemblyQualifiedName");

            int currentIndex = 0;
            string currentName = nameProp.stringValue;
            if (!string.IsNullOrEmpty(currentName))
            {
                Type currentType = Type.GetType(currentName);
                if (currentType != null)
                {
                    currentIndex = Array.IndexOf(types, currentType);
                    if (currentIndex == -1) currentIndex = 0;
                }
            }

            int newIndex = EditorGUI.Popup(position, label, currentIndex, typeNames);

            if (newIndex != currentIndex)
            {
                nameProp.stringValue = newIndex == 0 ? string.Empty : types[newIndex].AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
