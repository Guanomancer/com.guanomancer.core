using UnityEngine;
using UnityEditor;

namespace Guanomancer.Editor
{
    [CustomEditor(typeof(DocIt))]
    public class DocItEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DocIt docit = (DocIt)target;
            
            // 1. Draw your custom "Documentation View"
            RenderDocumentationView(docit);

            // 2. Add the Toggle Button
            GUILayout.Space(20);
            string buttonText = docit.isEditing ? "Hide Editor" : "Edit Content";

            if (GUILayout.Button(buttonText))
            {
                // Toggle the state and record for Undo purposes
                Undo.RecordObject(docit, "Toggle DocIt Editor");
                docit.isEditing = !docit.isEditing;
                EditorUtility.SetDirty(docit);
            }

            // 3. Conditionally Draw Default Inspector
            if (docit.isEditing)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Editor Mode Active", MessageType.Info);
                DrawDefaultInspector();
            }
        }

        private static void RenderDocumentationView(DocIt docit)
        {
            if (docit.headerImage != null)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float ratio = (float)docit.headerImage.height / (float)docit.headerImage.width;
                float height = width * ratio;

                Rect rect = GUILayoutUtility.GetRect(width, height);
                GUI.DrawTexture(rect, docit.headerImage, ScaleMode.ScaleToFit);
            }

            GUILayout.Space(10);
            GUILayout.Label(docit.title, EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (docit.sections != null)
            {
                foreach (var section in docit.sections)
                {
                    GUILayout.Label(section.heading, EditorStyles.boldLabel);

                    GUIStyle bodyStyle = new GUIStyle(EditorStyles.label);
                    bodyStyle.wordWrap = true;

                    GUILayout.Label(section.content, bodyStyle);

                    if (!string.IsNullOrEmpty(section.url))
                    {
                        if (GUILayout.Button(section.linkText ?? "Read More"))
                        {
                            Application.OpenURL(section.url);
                        }
                    }

                    GUILayout.Space(15);
                }
            }
        }
    }
}