using UnityEngine;

namespace Guanomancer
{
    public interface IInteraction<T>
    {
        bool CanInteract(T info);
        bool TryInteract(T info);

        public static bool Can(GameObject go, T info) =>
            go.TryGetComponent<IInteraction<T>>(out var interaction) && interaction.CanInteract(info);

        public static bool Can(GameObject go, T info, out IInteraction<T> interaction) =>
            go.TryGetComponent(out interaction) && interaction.CanInteract(info);

        public static bool Try(GameObject go, T info) =>
            go.TryGetComponent<IInteraction<T>>(out var interaction) && interaction.TryInteract(info);

        public static bool Try(GameObject go, T info, out IInteraction<T> interaction) =>
            go.TryGetComponent(out interaction) && interaction.TryInteract(info);

        public static bool Can(Component component, T info) =>
            component.gameObject.TryGetComponent<IInteraction<T>>(out var interaction) && interaction.CanInteract(info);

        public static bool Can(Component component, T info, out IInteraction<T> interaction) =>
            component.gameObject.TryGetComponent(out interaction) && interaction.CanInteract(info);

        public static bool Try(Component component, T info) =>
            component.gameObject.TryGetComponent<IInteraction<T>>(out var interaction) && interaction.TryInteract(info);

        public static bool Try(Component component, T info, out IInteraction<T> interaction) =>
            component.gameObject.TryGetComponent(out interaction) && interaction.TryInteract(info);
    }
}