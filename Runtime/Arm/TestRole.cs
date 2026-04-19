using System;
using UnityEngine;

namespace Guanomancer
{
    [CreateAssetMenu(fileName = "New Test Role", menuName = "Guanomancer/Roles/Test Role")]
    [Serializable]
    public class TestRole : Role<TestRole, TestRoleState>
    {
        public override Role OnEnter(ref TestRoleState state)
        {
            Debug.Log($"Enter {name}", this);
            return this;
        }

        public override void OnExit(ref TestRoleState state)
        {
            Debug.Log($"Exit {name}", this);
        }

        public override Role OnUpdate(ref TestRoleState state)
        {
            Debug.Log($"Update {name}", this);
            return this;
        }
    }

    [Serializable]
    public struct TestRoleState { }
}
