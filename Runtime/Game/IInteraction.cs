using UnityEngine;

namespace Guanomancer
{
    public interface IInteraction<T>
    {
        bool CanInteract(T info);
        bool TryInteract(T info);

        public static bool Can(GameObject go, out IInteraction<T> interaction) => Can(go, default, out interaction);
        public static bool Can(GameObject go, T info = default)
        {
            var interaction = go.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.CanInteract(info);
        }

        public static bool Can(GameObject go, T info, out IInteraction<T> interaction)
        {
            interaction = go.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.CanInteract(info);
        }

        public static bool Try(GameObject go, out IInteraction<T> interaction) => Try(go, default, out interaction);
        public static bool Try(GameObject go, T info = default)
        {
            var interaction = go.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.TryInteract(info);
        }

        public static bool Try(GameObject go, T info, out IInteraction<T> interaction)
        {
            interaction = go.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.TryInteract(info);
        }

        public static bool Can(Component component, out IInteraction<T> interaction) => Can(component, out interaction);
        public static bool Can(Component component, T info = default)
        {
            var interaction = component.gameObject.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.CanInteract(info);
        }

        public static bool Can(Component component, T info, out IInteraction<T> interaction)
        {
            interaction = component.gameObject.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.CanInteract(info);
        }

        public static bool Try(Component component, out IInteraction<T> interaction) => Try(component, out interaction);
        public static bool Try(Component component, T info = default)
        {
            var interaction = component.gameObject.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.TryInteract(info);
        }

        public static bool Try(Component component, T info, out IInteraction<T> interaction)
        {
            interaction = component.gameObject.GetComponentInParent<IInteraction<T>>();
            return interaction != null && interaction.TryInteract(info);
        }
    }
}