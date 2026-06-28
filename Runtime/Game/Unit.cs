using UnityEngine;

namespace Guanomancer
{
    public class Unit : MonoBehaviour
    {
        [field: SerializeField] public TagContainer<UnitTag> Tags { get; private set; } = new();

        public static string[] GetNamesOfUnits(Unit[] units)
        {
            string[] names = new string[units.Length];
            for (int i = 0; i < units.Length; i++)
            {
                names[i] = units[i].name;
            }
            return names;
        }

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
    }

    public abstract class UnitInteractionMonoBehaviour : MonoBehaviour, IUnitInteractionInfo
    {
        private Unit _unitCache;
        public Unit Unit
        {
            get
            {
                if (_unitCache == null)
                {
                    _unitCache = GetComponentInParent<Unit>(true);
                }
                return _unitCache;
            }
        }
    }

    public interface IUnitInteractionInfo
    {
        public Unit Unit { get; }
        GameObject gameObject { get; }
        string name { get; }
    }

    public interface IInteractionInfo<T> : IUnitInteractionInfo where T : struct
    {
        bool CanInteract(T interactionData);
        bool TryInteract(T interactionData) => TryInteract(ref interactionData);
        bool TryInteract(ref T interactionData);

    }
}