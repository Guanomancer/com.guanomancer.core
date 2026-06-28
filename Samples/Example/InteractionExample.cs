using UnityEngine;

namespace Guanomancer.Samples
{
    public class InteractionExample : UnitInteractionMonoBehaviour,
        IInteractionInfo<NoInteractionData>,
        IInteractionInfo<SomeInteractionData>
    {
        [Button("Can Interact")]
        public void BTN_CanInteract()
        {
            Debug.Log($"Can Interact (Some): {TryGetComponent<IInteractionInfo<SomeInteractionData>>(out var interaction) && interaction.CanInteract(new SomeInteractionData { })}");
            Debug.Log($"Can Interact (None): {TryGetComponent<IInteractionInfo<NoInteractionData>>(out var noInteraction) && noInteraction.CanInteract(new NoInteractionData { })}");
        }

        [Button("Try Get Valid Interactions")]
        public void BTN_TryGetValidInteractions()
        {
            Debug.Assert(Unit != null, $"{nameof(InteractionExample)} must be part of or a child of an {nameof(Guanomancer.Unit)}.", this);

            if (Unit.TryGetValidInteractions<SomeInteractionData>(new(), out var someInteraction, out var someCount))
            {
                foreach (var interaction in someInteraction) Debug.Log($"Valid: {nameof(SomeInteractionData)} ({interaction.name})");
            }
            else
            {
                Debug.Log($"No valid {nameof(SomeInteractionData)}");
            }
            if (Unit.TryGetValidInteractions<NoInteractionData>(new(), out var noInteractions, out var noCount))
            {
                foreach (var interaction in noInteractions) Debug.Log($"Valid: {nameof(NoInteractionData)} ({interaction.name})");
            }
            else
            {
                Debug.Log($"No valid {nameof(NoInteractionData)}");
            }
        }

        public bool CanInteract(SomeInteractionData interactionData) => true;
        public bool CanInteract(NoInteractionData interactionData) => false;
        public bool TryInteract(ref SomeInteractionData interactionData) => true;
        public bool TryInteract(ref NoInteractionData interactionData) => false;

    }

    public struct NoInteractionData { }
    public struct SomeInteractionData { }
}