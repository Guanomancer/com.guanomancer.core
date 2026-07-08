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
        // Cache the types per interface to keep the Inspector running smoothly
        private static readonly Dictionary<Type, Type[]> _typesCache = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, GUIContent[]> _namesCache = new Dictionary<Type, GUIContent[]>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type fieldType = fieldInfo.FieldType;

            // Safely extract the type if this field is inside an Array or List
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            // Get the 'T' from TypeInfo<T>
            Type targetInterface = fieldType.GetGenericArguments()[0];

            // Populate the cache for this specific interface if we haven't yet
            if (!_typesCache.ContainsKey(targetInterface))
            {
                // TypeCache searches ALL loaded assemblies automatically
                var foundTypes = TypeCache.GetTypesDerivedFrom(targetInterface)
                    .Where(t => /*t.IsValueType &&*/ !t.IsAbstract) // "Restrict to structs only" commented out
                    .ToList();

                foundTypes.Insert(0, null); // Add a "(None)" option at the top

                _typesCache[targetInterface] = foundTypes.ToArray();
                _namesCache[targetInterface] = foundTypes
                    .Select(t => new GUIContent(t == null ? "(None)" : t.Name))
                    .ToArray();
            }

            Type[] types = _typesCache[targetInterface];
            GUIContent[] typeNames = _namesCache[targetInterface];

            SerializedProperty nameProp = property.FindPropertyRelative("_assemblyQualifiedName");

            // Figure out which index in our dropdown is currently selected
            int currentIndex = 0;
            string currentName = nameProp.stringValue;
            if (!string.IsNullOrEmpty(currentName))
            {
                Type currentType = Type.GetType(currentName);
                if (currentType != null)
                {
                    currentIndex = Array.IndexOf(types, currentType);
                    if (currentIndex == -1) currentIndex = 0; // Fallback to (None) if type was deleted/renamed
                }
            }

            // Draw the dropdown
            int newIndex = EditorGUI.Popup(position, label, currentIndex, typeNames);

            // Apply changes if the user picked a new option
            if (newIndex != currentIndex)
            {
                nameProp.stringValue = newIndex == 0 ? string.Empty : types[newIndex].AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
