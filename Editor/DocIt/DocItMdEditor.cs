using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Guanomancer.Editor
{
    [CustomEditor(typeof(DocItMd))]
    public class DocItMdEditor : UnityEditor.Editor
    {
        private Vector2 scrollPos;
        private Dictionary<string, float> headerPositions = new Dictionary<string, float>();
        private string targetHeader = null;

        // Persistent UI states
        private bool showToC = true;

        public override void OnInspectorGUI()
        {
            DocItMd docit = (DocItMd)target;

            if (!docit.isEditing)
            {
                // 1. ToC Header & Toggle
                EditorGUILayout.BeginHorizontal();
                showToC = EditorGUILayout.Foldout(docit.isTocFolded, "Table of Contents", true, EditorStyles.foldoutHeader);
                if (showToC != docit.isTocFolded)
                {
                    docit.isTocFolded = showToC;
                    EditorUtility.SetDirty(docit);
                }

                if (scrollPos.y > 200)
                {
                    try
                    {
                        if (GUILayout.Button("↑ Back to Top", EditorStyles.miniButton, GUILayout.Width(100)))
                            scrollPos = Vector2.zero;
                    }
                    catch { }
                }
                EditorGUILayout.EndHorizontal();

                if (showToC)
                {
                    RenderTableOfContents(docit.markdownContent);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 2. Scroll View
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(600));
                EditorGUILayout.BeginVertical();
                RenderLineByLine(docit);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                // 3. Jump Logic
                if (!string.IsNullOrEmpty(targetHeader) && headerPositions.ContainsKey(targetHeader))
                {
                    scrollPos.y = headerPositions[targetHeader] - 10;
                    targetHeader = null;
                    Repaint();
                }
            }
            else
            {
                DrawDefaultInspector();
            }

            // 4. Toggle Editor Button
            GUILayout.Space(10);
            GUI.backgroundColor = docit.isEditing ? Color.red : Color.white;
            if (GUILayout.Button(docit.isEditing ? "Close & Save" : "Edit Content", GUILayout.Height(30)))
            {
                docit.isEditing = !docit.isEditing;
                EditorUtility.SetDirty(docit);
            }
            GUI.backgroundColor = Color.white;
        }

        private void RenderLineByLine(DocItMd docit)
        {
            string[] lines = docit.markdownContent.Split('\n');

            if (Event.current.type == EventType.Repaint)
                headerPositions.Clear();

            bool isInsideCodeBlock = false;
            List<string> currentCodeBlock = new();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) { GUILayout.Space(8); continue; }

                if (line.Trim().StartsWith("'''"))
                {
                    if (isInsideCodeBlock)
                    {
                        RenderCodeBlock(currentCodeBlock);
                        currentCodeBlock.Clear();
                        isInsideCodeBlock = false;
                    }
                    else
                    {
                        isInsideCodeBlock = true;
                    }
                }

                if (isInsideCodeBlock)
                {
                    currentCodeBlock.Add(line);
                    continue;
                }

                // Dividers
                if (Regex.IsMatch(line, @"^---+\s*$"))
                {
                    Rect rect = EditorGUILayout.GetControlRect(false, 10);
                    rect.height = 1;
                    rect.y += 4;
                    EditorGUI.DrawRect(rect, new Color(0.8f, 0.8f, 0.8f, 1f));
                    continue;
                }

                // Images
                var imgMatch = Regex.Match(line, @"^!\[(.*?)\]\((.*?)\)$");
                if (imgMatch.Success)
                {
                    RenderImage(imgMatch.Groups[1].Value, imgMatch.Groups[2].Value);
                    continue;
                }

                // Checkboxes (Interactive Custom Symbols)
                var taskMatch = Regex.Match(line, @"^([\s\*\-]*?)\[([ xX])\]\s+(.*)$");
                if (taskMatch.Success)
                {
                    RenderColoredCheckbox(docit, taskMatch, i);
                    continue;
                }

                // Headers
                var headerMatch = Regex.Match(line, @"^(#{1,6})\s*(.*)$");
                if (headerMatch.Success)
                {
                    RenderHeader(headerMatch);
                    continue;
                }

                // Body Text

                string processed = ProcessMarkdownLine(line);
                GUIStyle style = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };

                var linkMatch = Regex.Match(line, @"\[(.*?)\]\((.*?)\)");

                if (linkMatch.Success)
                {
                    EditorGUILayout.BeginHorizontal();
                    // Render the link as a clickable button
                    if (GUILayout.Button(linkMatch.Groups[1].Value, EditorStyles.linkLabel))
                    {
                        HandleLink(linkMatch.Groups[2].Value);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    float height = style.CalcHeight(new GUIContent(processed), EditorGUIUtility.currentViewWidth - 50);
                    EditorGUILayout.SelectableLabel(processed, style, GUILayout.Height(height));
                }
            }
        }

        private void RenderCodeBlock(List<string> lines)
        {
            string fullCode = string.Join("\n", lines);

            // Create a "Code" style: Dark background, Monospaced, Padding
            GUIStyle codeBoxStyle = new GUIStyle(EditorStyles.helpBox);
            //codeBoxStyle.normal.background = Texture2D.linearGrayTexture;
            codeBoxStyle.padding = new RectOffset(10, 10, 10, 10);
            codeBoxStyle.margin = new RectOffset(5, 5, 5, 5);

            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.font = EditorStyles.textField.font;
            textStyle.richText = true;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            EditorGUILayout.BeginVertical(codeBoxStyle);
            EditorGUILayout.SelectableLabel(fullCode, textStyle, GUILayout.Height(textStyle.CalcHeight(new GUIContent(fullCode), EditorGUIUtility.currentViewWidth - 60)));
            EditorGUILayout.EndVertical();
        }

        private void RenderImage(string altText, string path)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                float maxInspectorWidth = EditorGUIUtility.currentViewWidth - 40f;
                float renderWidth = Mathf.Min(tex.width, maxInspectorWidth);
                float aspectRatio = (float)tex.height / tex.width;
                float renderHeight = renderWidth * aspectRatio;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent(tex, altText), GUIStyle.none, GUILayout.Width(renderWidth), GUILayout.Height(renderHeight)))
                {
                    Selection.activeObject = tex;
                    EditorGUIUtility.PingObject(tex);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"Image not found: {path}", MessageType.Warning);
            }
        }

        private void HandleLink(string target)
        {
            if (target.StartsWith("http"))
            {
                Application.OpenURL(target);
            }
            else
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(target);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset); // Highlights it in yellow in Project view
                }
                else
                {
                    Debug.LogWarning($"DocIt: Could not find asset at {target}");
                }
            }
        }

        private void RenderColoredCheckbox(DocItMd docit, Match match, int lineIndex)
        {
            bool isChecked = match.Groups[2].Value.ToLower() == "x";
            string content = match.Groups[3].Value;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);

            // Custom Colored Checkbox Button
            // Cornflower Blue: #6495ED | Green: #5ce65c
            string color = isChecked ? "#5ce65c" : "#6495ED";
            string symbol = isChecked ? "\u2611" : "\u2610";

            GUIStyle symbolStyle = new GUIStyle(EditorStyles.label) { richText = true, fontSize = 16 };
            if (GUILayout.Button($"<color={color}>{symbol}</color>", symbolStyle, GUILayout.Width(25)))
            {
                ToggleCheckbox(docit, lineIndex, !isChecked);
            }

            GUIStyle taskStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            string text = isChecked ? $"<color=grey><s>{content}</s></color>" : content;
            EditorGUILayout.LabelField(text, taskStyle, GUILayout.Height(taskStyle.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - 65)));
            EditorGUILayout.EndHorizontal();
        }

        private void RenderHeader(Match match)
        {
            string title = match.Groups[2].Value;
            int level = match.Groups[1].Length;
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(25 + (6 - level)));

            if (Event.current.type == EventType.Repaint)
                headerPositions[title] = r.y;

            int fontSize = Mathf.Max(12, 22 - ((level - 1) * 2));
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true, fontSize = fontSize };
            EditorGUI.LabelField(r, title, headerStyle);
        }

        private void RenderTableOfContents(string content)
        {
            var matches = Regex.Matches(content, @"^(#{1,6})\s*(.*)$", RegexOptions.Multiline);
            if (matches.Count == 0) return;

            EditorGUI.indentLevel++;
            foreach (Match m in matches)
            {
                string title = m.Groups[2].Value;
                int indent = m.Groups[1].Length - 1;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indent * 15);
                if (GUILayout.Button("• " + title, EditorStyles.linkLabel))
                {
                    targetHeader = title;
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void ToggleCheckbox(DocItMd docit, int lineIndex, bool newState)
        {
            Undo.RecordObject(docit, "Toggle Checkbox");
            string[] lines = docit.markdownContent.Split('\n');
            lines[lineIndex] = Regex.Replace(lines[lineIndex], @"\[([ xX])\]", $"[{(newState ? "x" : " ")}]");
            docit.markdownContent = string.Join("\n", lines);
            EditorUtility.SetDirty(docit);
            AssetDatabase.SaveAssets();
        }

        private string ProcessMarkdownLine(string line)
        {
            string result = line;

            // A. Protection Pass (Escapes)
            result = result.Replace(@"\*", "\uE000");
            result = result.Replace(@"\#", "\uE001");
            result = result.Replace(@"\-", "\uE002");
            result = result.Replace(@"\.", "\uE003");
            result = result.Replace(@"\~", "\uE004");

            //// B. Horizontal Rule
            //result = Regex.Replace(result, @"^---+\s*$",
            //    "<color=grey>___________________________________________________________</color>",
            //    RegexOptions.Multiline);

            // C. Headers (Size 22 down to 12)
            result = Regex.Replace(result, @"^(#{1,6})\s*(.*)$", m =>
            {
                int hashCount = m.Groups[1].Length;
                int fontSize = Mathf.Max(12, 22 - ((hashCount - 1) * 2));
                return $"<size={fontSize}><b>{m.Groups[2].Value}</b></size>";
            }, RegexOptions.Multiline);

            // D. Task Lists (Refined for better matching)
            // Checked: Matches - [x], -[x], * [x], or *[x] (case insensitive)
            result = Regex.Replace(result, @"^[\*\-]\s?\[[xX]\]\s+(.*)$",
                "  <color=#5ce65c><b>\u2611</b></color>  <color=grey><s>$1</s></color>",
                RegexOptions.Multiline);

            // Unchecked: Matches - [ ], -[ ], * [ ], or *[ ]
            result = Regex.Replace(result, @"^[\*\-]\s?\[\s?\]\s+(.*)$",
                "  <color=#cccccc>\u2610</color>  $1",
                RegexOptions.Multiline);

            // E. Standard Lists
            result = Regex.Replace(result, @"^[\*\-]\s*(.*)$", "  • $1",
                RegexOptions.Multiline);
            result = Regex.Replace(result, @"^(\d+\.)\s*(.*)$", "  $1 $2",
                RegexOptions.Multiline);

            // F. Bold & Italics
            result = Regex.Replace(result, @"\*\*(.*?)\*\*", "<b>$1</b>");
            result = Regex.Replace(result, @"\*(.*?)\*", "<i>$1</i>");
            result = Regex.Replace(result, @"~~(.*?)~~", "<s>$1</s>");

            // G. Restoration Pass
            result = result.Replace("\uE000", "*");
            result = result.Replace("\uE001", "#");
            result = result.Replace("\uE002", "-");
            result = result.Replace("\uE003", ".");
            result = result.Replace("\uE004", "~");

            return result;
        }
    }

    public static class DocItMenuUtility
    {
        [MenuItem("Assets/DocIt/Create DocIt", false, 1)]
        public static void CreateDocItMarkdownAsset()
        {
            MonoScript script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<DocItMdEditor>());
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string directory = System.IO.Path.GetDirectoryName(scriptPath);

            // 2. Combine with the relative path to your icon
            // Assuming the icon is in the same folder as the script:
            string iconPath = System.IO.Path.Combine(directory, "DocItIcon.png");

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            // Fallback if not found
            if (icon == null) icon = EditorGUIUtility.IconContent("TextAsset Icon").image as Texture2D;

            var endNameEditAction = ScriptableObject.CreateInstance<CreateDocItMdAssetCreationEndAction>();

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                default(EntityId),
                endNameEditAction,
                "New DocIt.asset",
                icon,
                iconPath,
                true
            );
            }

            private class CreateDocItMdAssetCreationEndAction : AssetCreationEndAction
            {
            public override void Action(EntityId instanceId, string pathName, string resourceFile)
            {
                // 1. Create the actual instance
                DocItMd asset = ScriptableObject.CreateInstance<DocItMd>();

                // 2. Create the asset at the path provided by the rename box
                AssetDatabase.CreateAsset(asset, pathName);

                if (!string.IsNullOrEmpty(resourceFile))
                {
                    Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(resourceFile);
                    if (icon != null)
                    {
                        EditorGUIUtility.SetIconForObject(asset, icon);
                        asset.headerIconPath = resourceFile;
                        asset.SetDefaultContent();
                    }
                }

                AssetDatabase.SaveAssets();

                // 3. Select and highlight the new asset
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }

        // Priority 11 creates a small divider line between Create and Copy
        [MenuItem("Assets/DocIt/Copy Markdown Path", false, 11)]
        private static void CopyMarkdownPath()
        {
            Object selected = Selection.activeObject;
            if (selected == null) return;

            string path = AssetDatabase.GetAssetPath(selected);
            string output = (selected is Texture2D) ? $"![Label]({path})" : $"[Label]({path})";

            EditorGUIUtility.systemCopyBuffer = output;
            Debug.Log($"DocIt: Path copied: {output}");
        }
    }
}