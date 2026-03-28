using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Guanomancer.Editor
{
    [CustomEditor(typeof(Object), true)]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute)method.GetCustomAttribute(typeof(ButtonAttribute), true);
                if (buttonAttribute != null)
                {
                    string buttonLabel = string.IsNullOrEmpty(buttonAttribute.Label) ?
                        FormatMethodName(method) :
                        buttonAttribute.Label;
                    string buttonTooltip = string.IsNullOrEmpty(buttonAttribute.Tooltip) ?
                        $"Will invoke the method '{method.Name}' on '{target.GetType().Name}' ({target.name})." :
                        buttonAttribute.Tooltip;
                    var defaultColor = GUI.backgroundColor;
                    GUI.backgroundColor = defaultColor * buttonAttribute.TintColor;
                    if (GUILayout.Button(new GUIContent(buttonLabel, buttonTooltip)))
                    {
                        method.Invoke(target, null);
                    }
                    GUI.backgroundColor = defaultColor;
                }
            }

            DrawDefaultInspector();
        }

        private string FormatMethodName(MethodInfo method)
        {
            var name = new System.Text.StringBuilder();
            var methodName = method.Name;
            for (int i = 0; i < methodName.Length; i++)
            {
                char c = methodName[i];
                name.Append(
                    char.IsLower(c) && i == 0 ? char.ToUpper(c) :
                    char.IsUpper(c) && i > 0 ? " " + c :
                    c
                    );
            }
            return name.ToString();
        }
    }
}