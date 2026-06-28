using UnityEngine;
using Guanomancer;

namespace Guanomancer.Samples
{
    /// <summary>
    /// Provide a general description of the public class.
    /// </summary>
    /// <remarks>
    /// Packages require XmlDoc documentation for ALL Package APIs.
    /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments
    /// </remarks>
    public class SampleHierarchySettings : MonoBehaviour, IHierarchySettings
    {
        /// <summary>
        /// Provide a description of what this public method does.
        /// </summary>
        public Color ColorInHierarchy => Color.cornflowerBlue;

        bool IHierarchySettings.FullRowColoring => true;
    }
}