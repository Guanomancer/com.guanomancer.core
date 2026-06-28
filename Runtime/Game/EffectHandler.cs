// using System;
// using UnityEngine;

// namespace Guanomancer
// {
//     public abstract class EffectHandler : MonoBehaviour
//     {
//         [field: SerializeField] public Transform IconSocket { get; private set; }

//         public Actor Actor { get; private set; }

//         protected virtual void Awake()
//         {
//             Actor = transform.GetComponentInParent<Actor>();
//         }

//         // protected void TryApplyToolConfigurations(Actor toActor, Actor[] targetActors)
//         // {
//         //     foreach (var toolConfig in Game.Settings.ToolConfigurations)
//         //     {
//         //         if (toolConfig.Validate(targetActors))
//         //         {
//         //             toolConfig.ApplyTo(toActor, targetActors);
//         //             break;
//         //         }
//         //     }
//         // }
//     }

//     public interface IEffectHandler<T> where T : IEffectInfo
//     {
//         bool ApplyEffect(T effectInfo);
//     }

//     public interface IEffectInfo { }
// }