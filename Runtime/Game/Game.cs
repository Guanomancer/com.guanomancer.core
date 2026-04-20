using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guanomancer
{
    public class Game : GameComponent<Game>
    {
        public static void ResetAll()
        {
            var all = Current._components.ToArray();
            foreach (var gameComp in all)
            {
                if (!gameComp.IgnoreResetAll)
                    gameComp.ResetSelf(false);
            }
            Reset();
        }

        // [System.Diagnostics.Conditional("DEBUG")]
        // public static void Log(object message, Component component) => Log(message, component, component);
        // [System.Diagnostics.Conditional("DEBUG")]
        // public static void Log(object message, UnityEngine.Object context = null, Component component = null)
        // {
        //     if (context == null) context = component;
        //     else if (component == null && context is Component comp) component = comp;
        //     var frame = Time.frameCount;
        //     var compInfo = component == null ? "" : $"{PadClamp(component?.name, 16)} . {PadClamp(component?.GetType().Name, 24)}";
        //     Debug.Log($"[{frame.ToString("D5")}]<color=#44AAFF>{compInfo}</color>: {message}", context);

        //     string PadClamp(string name, int size) =>
        //         name.Substring(0, Math.Min(name.Length, size)).PadLeft(size);
        // }

        [SerializeField] bool _gameStateLogging;

        public bool IsStarted { get; private set; }

        private List<IGameComponent> _components = new();

        protected override void OnAfterReset() { }
        protected override void OnBeforeReset() { }
        protected override void OnAwake() { }
        protected override void OnStart() { }

        public override bool SkipRegisterWithGame => true;
        public override bool SkipRegisterGameStart => true;

        public void Register<T>(T gameComponent) where T : IGameComponent
        {
            this.Info($"Register {gameComponent.GetType().Name}");
            _components.Add(gameComponent);
        }

        public void Unregister<T>(T gameComponent) where T : IGameComponent
        {
            this.Info($"Unregister {gameComponent.GetType().Name}");
            _components.Remove(gameComponent);
        }

        void Start()
        {
            IsStarted = true;
            StartGame();
        }
    }

    public interface IGameComponent
    {
        void ResetSelf(bool reinstantiate);
        bool IgnoreResetAll { get; }
    }

    public abstract class GameComponent<T> : MonoBehaviour, IGameComponent
        where T : GameComponent<T>
    {
        private static T _currentInstance;
        public static T Current
        {
            get
            {
                if (_currentInstance == null && !(_currentInstance =
                    FindAnyObjectByType<T>(FindObjectsInactive.Include)))
                {
                    var obj = new GameObject("Game");
                    _currentInstance = obj.AddComponent<T>();
                }
                return _currentInstance;
            }
        }
        public static bool IsInitialized => _currentInstance != null;

        public static void Reset(bool reinstantiate = true)
        {
            var inst = _currentInstance;
            if (inst != null)
            {
                inst.OnBeforeReset();
                inst.BeforeReset?.Invoke();
                _currentInstance = null;
                inst.OnAfterReset();
                inst.AfterReset?.Invoke();
            }
            if (reinstantiate) return;
            inst = Current;
        }

        public void ResetSelf(bool reinstantiate = true) => Reset(reinstantiate);

        public event Action Awoken;
        public event Action Started;
        public event Action BeforeReset;
        public event Action AfterReset;

        // protected virtual void OnAwake() => this.Info("OnAwake");
        // protected virtual void OnStart() => this.Info("OnStart");
        // protected virtual void OnBeforeReset() => this.Info("OnBeforeReset");
        // protected virtual void OnAfterReset() => this.Info("OnAfterReset");

        protected abstract void OnAwake();
        protected abstract void OnStart();
        protected abstract void OnBeforeReset();
        protected abstract void OnAfterReset();

        public virtual bool IgnoreResetAll => false;
        public virtual bool SkipRegisterWithGame => false;
        public virtual bool SkipRegisterGameStart => false;

        protected void StartGame()
        {
            if (!(this is Game)) throw new NotImplementedException("Please don't tell a game component that is not Game itself to start the game.");

            this.Info("Starting Game");
            Logging.Indent();
            {
                OnStart();
                Started?.Invoke();
            }
            Logging.Unindent();
            this.Info("Game Started");
        }

        private void GameStarted()
        {
            Game.Current.Started -= GameStarted;

            OnStart();
            Started?.Invoke();
        }

        void Awake()
        {
            if (!SkipRegisterWithGame)
            {
                Game.Current.Register(this);
            }

            this.Info("Awoken", this);
            OnAwake();
            Awoken?.Invoke();

            if (!SkipRegisterGameStart)
            {
                if (Game.Current.IsStarted)
                {
                    GameStarted();
                }
                else
                {
                    Game.Current.Started += GameStarted;
                }
            }
        }

        void OnDestroy()
        {
            if (_currentInstance == this)
            {
                _currentInstance = null;
                if (Game.IsInitialized) Game.Current.Unregister(this);
            }
            if (!SkipRegisterGameStart)
            {
                if (Game.IsInitialized) Game.Current.Started -= GameStarted;
            }
        }
    }
}
