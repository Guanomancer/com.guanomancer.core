using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;

namespace Guanomancer
{
    public class Game
    {
        [SerializeField] IGameModule[] _modules;

        private static Dictionary<Type, IGameModule> _moduleTypes;

        public static void Reset() => ResetAll(true);

        public static void ResetAll(bool resetWithGameOnly = false)
        {
            var modules = _moduleTypes.Values.ToArray();
            for (int i = modules.Length; i >= 0; i++)
            {
                if (resetWithGameOnly && modules[i].ResetWithGame) continue;

                _moduleTypes.Remove(modules[i].GetType());
            }
        }

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // static void BeforeSceneLoad()
        // {
        //     _moduleTypes = new();
        //     if (_modules != null)
        //     {
        //         foreach (var modile in _modules)
        //         {

        //         }
        //     }
        // }
    }

    public interface IGameModule
    {
        bool ResetWithGame { get; }


        public abstract class GameModule<T> : ScriptableObject, IGameModule where T : GameModule<T>
        {
            public virtual bool ResetWithGame => true;
        }
    }
}