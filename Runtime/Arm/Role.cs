// using UnityEngine;
// using UnityEngine.Assertions;

// namespace Guanomancer
// {
//     [System.Serializable]
//     public abstract class Role : ScriptableObject
//     {
//         public abstract Role Enter();
//         public abstract Role Update();
//         public abstract void Exit();
//     }

//     [System.Serializable]
//     public abstract class Role<T, ST> : Role
//         where T : Role<T, ST> where ST : struct
//     {
//         public abstract Role OnUpdate(ref ST state);
//         public abstract Role OnEnter(ref ST state);
//         public abstract void OnExit(ref ST state);

//         public bool IsActive { get; private set; }
//         private ST _state;

//         public override Role Enter()
//         {
//             Assert.IsFalse(IsActive);

//             _state = new();
//             var enterState = OnEnter(ref _state);
//             if (enterState is Role<T, ST>)
//             {
//                 IsActive = true;
//             }
//             return enterState;
//         }

//         public override Role Update()
//         {
//             Assert.IsTrue(IsActive);
//             return OnUpdate(ref _state);
//         }

//         public override void Exit()
//         {
//             Assert.IsTrue(IsActive);
//             OnExit(ref _state);
//             IsActive = false;
//         }
//     }
// }
