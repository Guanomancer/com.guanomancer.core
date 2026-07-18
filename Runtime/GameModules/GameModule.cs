using System.Collections.Generic;
using UnityEngine;

namespace Guanomancer
{
    public abstract class GameModule<T> : ScriptableObject, IGameModule where T : GameModule<T>
    {
        protected static string DefaultResourcePath = System.IO.Path.Combine("GameModules", typeof(T).Name);

        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var original = Resources.Load<T>(DefaultResourcePath);
                    if (original != null)
                    {
                        _instance = Instantiate(original);
                        GameModuleManager.RegisterModule(_instance);
                    }
                }
                return _instance;
            }
        }

        void OnDestroy()
        {
            GameModuleManager.UnregisterModule<T>();
        }
    }

    public interface IGameModule { }

    static class GameModuleManager
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        // static void SubsystemRegistration() => Debug.Log("SubsystemRegistration");
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        // static void AfterAssembliesLoaded() => Debug.Log("AfterAssembliesLoaded");
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        // static void BeforeSplashScreen() => Debug.Log("BeforeSplashScreen");
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // static void BeforeSceneLoad() => Debug.Log("BeforeSceneLoad");
        // void Awake() => Debug.Log("Awake");
        // void OnEnable() => Debug.Log("OnEnable");
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // static void AfterSceneLoad() => Debug.Log("AfterSceneLoad");
        // void Start() => Debug.Log("Start");

        // void OnDisable() => Debug.Log("OnDisable");
        // void OnDestroy() => Debug.Log("OnDestroy");

        static Dictionary<System.Type, IGameModule> _modules;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            _modules = new();
        }

        public static void RegisterModule<T>(T module) where T : GameModule<T>, IGameModule
        {
            if (_modules.ContainsKey(typeof(T)))
            {
                Log.Warn(null, $"Replacing the registration of {typeof(T).Name} : {nameof(IGameModule)} with another instance.");
                _modules.Remove(typeof(T));
            }
            _modules.Add(typeof(T), module);
            Log.Info(null, $"Registered {typeof(T).Name} : {nameof(IGameModule)}.");
        }

        public static void UnregisterModule<T>() where T : GameModule<T>, IGameModule
        {
            if (!_modules.Remove(typeof(T)))
            {
                Log.Warn(null, $"Attempted to unregister a {nameof(T)} : {nameof(IGameModule)} but it is not currently registered.");
            }
            else
            {
                Log.Info(null, $"Unregistered {typeof(T).Name} : {nameof(IGameModule)}.");
            }
        }
    }
}