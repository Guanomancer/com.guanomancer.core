using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Guanomancer.Editor
{
    [InitializeOnLoad]
    public class HierarchyHighlighting
    {
        static HierarchyHighlighting()
        {
#if UNITY_6000_6_OR_NEWER
            HierarchyWindow.BindViewItem += OnBindViewItem;
#else
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += HierarchyWindowItemOnGUI;
#endif
        }

        private static List<IHierarchySettings> _hierarchySettings = new();

#if UNITY_6000_6_OR_NEWER
        private static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            item.style.backgroundColor = new StyleColor();
            if (!(item.Handler is HierarchyGameObjectHandler handler)) return;
            var entityId = handler.GetEntityId(item.Node);
            if (!(EditorUtility.EntityIdToObject(entityId) is GameObject gameObject)) return;
            gameObject.GetComponents(_hierarchySettings);
            if (_hierarchySettings.Count == 1 && _hierarchySettings[0].FullRowColoring)
            {
                var highlightColor = _hierarchySettings[0].ColorInHierarchy;
                highlightColor.a = gameObject.activeInHierarchy ? .1f : .03f;
                item.style.backgroundColor = new StyleColor(highlightColor);
            }
            else if (_hierarchySettings.Count > 0)
            {
                for (int i = 0; i < _hierarchySettings.Count; i++)
                {
                    var highlightColor = _hierarchySettings[_hierarchySettings.Count - i - 1].ColorInHierarchy;
                    highlightColor.a = gameObject.activeInHierarchy ? .1f : .03f;
                    item.style.backgroundColor = new StyleColor(highlightColor);
                }
            }
            else
            {
                var components = gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (string.IsNullOrEmpty(component?.GetType()?.Namespace))
                    {
                        item.style.backgroundColor = new StyleColor(new Color(.2f, .6f, 1f, (
                                gameObject.activeInHierarchy ? 0.08f : 0.05f
                            )));
                        return;
                    }
                }
            }
        }
#else
        private static void HierarchyWindowItemOnGUI(EntityId entityId, Rect selectionRect)
        {
            if (EditorUtility.EntityIdToObject(entityId) is GameObject gameObject)
            {
                gameObject.GetComponents<IHierarchySettings>(_hierarchySettings);
                if (_hierarchySettings.Count > 0)
                {
                    if (_hierarchySettings[0].FullRowColoring && _hierarchySettings.Count == 1)
                    {
                        var highlightColor = _hierarchySettings[0].ColorInHierarchy;
                        highlightColor.a = gameObject.activeInHierarchy ? .1f : .03f;
                        HighlightItem(gameObject, selectionRect,
                            highlightColor
                            );
                    }
                    else
                    {
                        var rects = GetSquareRects(selectionRect, _hierarchySettings.Count);
                        for (int i = 0; i < _hierarchySettings.Count; i++)
                        {
                            var highlightColor = _hierarchySettings[_hierarchySettings.Count - i - 1].ColorInHierarchy;
                            highlightColor.a = gameObject.activeInHierarchy ? .1f : .03f;
                            HighlightItem(gameObject, rects[i],
                                highlightColor
                                );
                        }
                    }
                }
                else
                {
                    var components = gameObject.GetComponents<MonoBehaviour>();
                    foreach (var component in components)
                    {
                        if (string.IsNullOrEmpty(component?.GetType()?.Namespace))
                        {
                            HighlightItem(gameObject, selectionRect,
                                new Color(1f, 1f, 1f, (
                                    gameObject.activeInHierarchy ? 0.08f : 0.05f
                                )));
                            return;
                        }
                    }
                }
            }
        }

        private static void HighlightItem(GameObject gameObject, Rect rect, Color color)
            => EditorGUI.DrawRect(rect, color);

        private static Rect[] GetSquareRects(Rect selectionRect, int count)
        {
            if (count <= 0)
                return System.Array.Empty<Rect>();

            float size = selectionRect.height;
            float insetSize = size - 2;
            Rect[] rects = new Rect[count];

            float startX = selectionRect.xMax - size;

            for (int i = 0; i < count; i++)
            {
                float x = startX - i * size;
                rects[i] = new Rect(x, selectionRect.y, insetSize, insetSize);
            }

            return rects;
        }
#endif
    }
}