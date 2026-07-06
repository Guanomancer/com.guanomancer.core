using UnityEngine;

namespace Guanomancer
{
    public static class Log
    {
        private const string FRAME_COUNT_CHARACTERS = "D3";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeFirstSceneLoaded()
        {
            _indents = 0;
            _lastLogFrame = 0;
            _frameCounterTone = false;
        }
        static int _indents;
        static int _lastLogFrame;
        static bool _frameCounterTone;
        public static void Indent() => _indents++;
        public static void Unindent() => _indents = System.Math.Clamp(_indents - 1, 0, int.MaxValue);


        [System.Diagnostics.Conditional("DEBUG")]
        [HideInCallstack]
        public static void Info(this Object self, object message, Object context = null)
        {
            if (context == null) context = self;
            
            var frame = Time.frameCount;
            if(frame != _lastLogFrame)
            {
                _lastLogFrame = frame;
                _frameCounterTone = !_frameCounterTone;
            }
            var toneColor = _frameCounterTone ? "AA" : "88";
            var frameInfo = $"<color=#CCCCCC>[{frame.ToString(FRAME_COUNT_CHARACTERS)}]</color>";
            var compInfo = self == null ? "" : $"<color=#88{toneColor}FF>{self?.name}.{self?.GetType().Name}</color>: ";
            Debug.Log($"{frameInfo}{compInfo}{new string(' ', _indents * 2)}{message}", context);
        }
    }
}
