// using UnityEngine;

// namespace Guanomancer
// {
//     public abstract class Affector<T> : MonoBehaviour, IAffector where T : IEffectInfo, new()
//     {
//         [field: SerializeField] public GameObject IconPrefab { get; private set; }
//         public abstract bool ApplyFunction(Actor[] actors);

//         public bool CanApplyTo(Actor[] actors)
//         {
//             T bluntHitEffect = new();
//             var type = typeof(T);
//             for (int i = 0; i < actors.Length; i++)
//             {
//                 Actor actor = actors[i];
//                 if (actor.HasEffectHandler<T>()) return true;
//             }
//             return false;
//         }
//     }

//     public interface IAffector
//     {
//         string name { get; }
//         public GameObject IconPrefab { get; }
//         bool ApplyFunction(Actor[] actors);
//         bool CanApplyTo(Actor[] actors);
//     }
// }