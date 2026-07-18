using UnityEngine;
using UnityEditor;

namespace Guanomancer.Samples
{
    public class Game : GameModule<Game>
    {
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad() => Debug.Assert(Instance != null);
    }

    public static class GameModuleMenu
    {
        [MenuItem("Guanomancer/Create Game Module")]
        static void CreateGameModule()
        {
            string resourcesPath = "Assets/Resources/GameModules";
            string assetPath = $"{resourcesPath}/{nameof(Game)}.asset";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "GameModules");
            }
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            Game newAsset = ScriptableObject.CreateInstance<Game>();
            AssetDatabase.CreateAsset(newAsset, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newAsset;
        }
    }
}