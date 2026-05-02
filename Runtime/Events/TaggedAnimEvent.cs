using UnityEngine;
using UnityEngine.Events;

public class TaggedAnimEvent : MonoBehaviour
{
    [SerializeField] TaggedAnimEventEntry[] _events;

    public void AnimEvent(AnimEventTag tag)
    {
        foreach(var entry in _events)
        {
            if (entry.Tag == tag)
            {
                entry.Event.Invoke();
            }
        }
    }
}

[System.Serializable]
public struct TaggedAnimEventEntry
{
    public AnimEventTag Tag;
    public UnityEvent Event;
}
