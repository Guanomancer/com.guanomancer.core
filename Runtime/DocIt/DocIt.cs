using UnityEngine;

namespace Guanomancer
{
    [CreateAssetMenu(fileName = "New DocIt", menuName = "Project/DocIt")]
    public class DocIt : ScriptableObject
    {
        [HideInInspector] public bool isEditing = false;

        public Texture2D headerImage;
        public string title;

        [System.Serializable]
        public class Section
        {
            public string heading;
            [TextArea(5, 10)]
            public string content;
            public string linkText;
            public string url;
        }

        public Section[] sections;
    }
}