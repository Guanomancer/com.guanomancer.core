using System.Collections;
using UnityEngine;

namespace Guanomancer
{
    public class Pinger : GameComponent<Pinger>
    {
        protected override void OnAfterReset() { }
        protected override void OnAwake() { }
        protected override void OnBeforeReset() { }
        protected override void OnStart() { }

        void Start()
        {
            StartCoroutine(__CO());

            IEnumerator __CO()
            {
                while (true)
                {
                    int c = Random.Range(1, 6);
                    for (int i = 0; i < c; i++) yield return null;
                    this.Info($"pingpong {c} {Time.frameCount} {Time.renderedFrameCount} {Time.captureFramerate}");
                }
            }
        }
    }
}