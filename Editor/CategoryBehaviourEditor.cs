using UnityEngine;
using UnityEditor;

namespace Guanomancer.Editor
{
    public class CategoryBehaviourEditor : EditorWindow
    {
        [MenuItem("GameObject/Guanomancer/Create Category Object", false, 10)]
        private static void CreateCategoryObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("New Category");
            go.AddComponent<CategoryBehaviour>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}