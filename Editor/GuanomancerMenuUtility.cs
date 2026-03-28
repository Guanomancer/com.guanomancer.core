using UnityEditor;
using UnityEngine;

namespace Guanomancer.Editor
{
    public static class GuanomancerMenuUtility
    {
        [MenuItem("GameObject/Guanomancer/Create Category Object", false, 10)]
        private static void CreateCategoryObject(MenuCommand menuCommand)
        {
            // 1. Create a new GameObject
            GameObject go = new GameObject("New Category");
            
            // 2. Add the specific behaviour
            go.AddComponent<CategoryBehaviour>();

            // 3. Ensure it gets reparented correctly if right-clicking an existing object
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // 4. Register the creation in the Undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            // 5. Select the new object
            Selection.activeObject = go;
        }
    }
}
