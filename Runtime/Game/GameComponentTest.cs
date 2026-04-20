using UnityEngine;

namespace Guanomancer
{
    public class GameComponentTest : GameComponent<GameComponentTest>
    {
        protected override void OnBeforeReset() { }
        protected override void OnAfterReset() { }
        protected override void OnAwake() { }
        protected override void OnStart() { }
    }
}