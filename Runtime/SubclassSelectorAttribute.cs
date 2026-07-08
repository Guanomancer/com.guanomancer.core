using System;
using UnityEngine;

namespace Guanomancer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}