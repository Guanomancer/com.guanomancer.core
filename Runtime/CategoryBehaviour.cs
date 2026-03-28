using UnityEngine;

namespace Guanomancer
{
    public class CategoryBehaviour : MonoBehaviour, IHierarchySettings
    {
        const string CATEGORY_INDENT = "----- ----- ----- -----{ ";

        [field: SerializeField] public Color ColorInHierarchy { get; private set; }

        public bool FullRowColoring => true;

        public string CategoryName => name.StartsWith(CATEGORY_INDENT) ? name.Substring(CATEGORY_INDENT.Length) : name;

        [Button()] public void ToggleActive() => gameObject.SetActive(!gameObject.activeSelf);

        private void OnValidate()
        {
            gameObject.name = name.StartsWith(CATEGORY_INDENT) ? name.ToUpper() : CATEGORY_INDENT + name.ToUpper();
        }
    }

    public interface IHierarchySettings
    {
        Color ColorInHierarchy { get; }
        bool FullRowColoring { get; }
    }
}