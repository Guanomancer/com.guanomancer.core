using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guanomancer
{
    public class Actor : MonoBehaviour
    {
        public bool TryGetValidInteractions<T>(T interactionData, out IInteractionInfo<T>[] interactions, out int count) where T : struct
        {
            count = 0;
            interactions = GetComponentsInChildren<IInteractionInfo<T>>(true);
            if (interactions.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < interactions.Length; i++)
            {
                var interaction = interactions[i];
                if (interaction.CanInteract(interactionData))
                {
                    interactions[count] = interaction;
                    count++;
                }
            }
            for (int i = count; i < interactions.Length; i++)
            {
                interactions[i] = null;
            }
            return count > 0;
        }

        public bool TryInteract<T>(T interactionData, out IInteractionInfo<T>[] interactions, out int count) where T : struct
        {
            count = 0;
            if (!TryGetValidInteractions(interactionData, out interactions, out var interactionsCount)) return false;

            for (int i = 0; i < interactionsCount; i++)
            {
                var interaction = interactions[i];
                if (interaction.TryInteract(ref interactionData))
                {
                    interactions[count] = interaction;
                    count++;
                }
            }
            for (int i = count; i < interactionsCount; i++)
            {
                interactions[i] = null;
            }
            return count > 0;
        }

        [field: SerializeField] public TagContainer<ActorTag> Tags { get; private set; }

        // private Dictionary<Type, object> _effectHandlers = new();

        // public bool HasEffectHandler<T>(out EffectHandler response) where T : EffectHandler, IEffectInfo
        // {
        //     response = null;
        //     if (!TryGetComponent(out T comp)) return false;

        //     response = comp;
        //     return true;
        // }

        // public void AddEffectHandler<T>(IEffectHandler<T> effectHandler) where T : IEffectInfo
        // {
        //     var type = typeof(T);
        //     List<IEffectHandler<T>> effectHandlerList;
        //     if (_effectHandlers.TryGetValue(type, out var effectHandlerListObject))
        //     {
        //         effectHandlerList = (List<IEffectHandler<T>>)effectHandlerListObject;
        //         effectHandlerList.Add(effectHandler);
        //     }
        //     else
        //     {
        //         effectHandlerList = new() { effectHandler };
        //         _effectHandlers.Add(type, effectHandlerList);
        //     }
        // }

        // public void RemoveEffectHandler<T>(IEffectHandler<T> effectHandler) where T : IEffectInfo
        // {
        //     var type = typeof(T);
        //     if (!_effectHandlers.TryGetValue(type, out var effectHandlerListObject)) return;

        //     var effectHandlerList = (List<IEffectHandler<T>>)effectHandlerListObject;
        //     effectHandlerList.Remove(effectHandler);
        // }

        // public bool HasEffectHandler<T>() where T : IEffectInfo => _effectHandlers.ContainsKey(typeof(T));

        // public bool ApplyEffect<T>(T effectInfo) where T : IEffectInfo
        // {
        //     var type = typeof(T);
        //     if (!_effectHandlers.TryGetValue(type, out var effectHandlerListObject)) return false;

        //     var effectHandlerList = (List<IEffectHandler<T>>)effectHandlerListObject;
        //     bool effectApplied = false;
        //     for (int i = effectHandlerList.Count - 1; i >= 0; i--)
        //     {
        //         effectApplied |= effectHandlerList[i].ApplyEffect(effectInfo);
        //     }
        //     return effectApplied;
        // }

        public static string[] GetNamesOfActors(Actor[] actors)
        {
            string[] names = new string[actors.Length];
            for (int i = 0; i < actors.Length; i++)
            {
                names[i] = actors[i].name;
            }
            return names;
        }
    }

    public abstract class ActorInteractionMonoBehaviour : MonoBehaviour, IActorInteractionInfo
    {
        private Actor _actorCache;
        public Actor Actor
        {
            get
            {
                if (_actorCache == null)
                {
                    _actorCache = GetComponentInParent<Actor>(true);
                }
                return _actorCache;
            }
        }
    }

    public interface IActorInteractionInfo
    {
        public Actor Actor { get; }
        GameObject gameObject { get; }
        string name { get; }
    }

    public interface IInteractionInfo<T> : IActorInteractionInfo where T : struct
    {
        bool CanInteract(T interactionData);
        bool TryInteract(T interactionData) => TryInteract(ref interactionData);
        bool TryInteract(ref T interactionData);

    }
}