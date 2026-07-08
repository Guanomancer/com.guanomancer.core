using System;
using UnityEngine;

namespace Guanomancer
{
    [Serializable]
    public class TypeInfo<T>
    {
        [SerializeField] private string _assemblyQualifiedName;

        public Type Type => string.IsNullOrEmpty(_assemblyQualifiedName)
            ? null
            : Type.GetType(_assemblyQualifiedName);
    }
}
