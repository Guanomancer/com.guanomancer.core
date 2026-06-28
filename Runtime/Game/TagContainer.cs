using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Guanomancer
{
    [System.Serializable]
    public class TagContainer<T> where T : TagBase
    {
        [field: SerializeField] public Dictionary<T, float> Tags { get; private set; } = new();

        public void Add(T tag)
        {
            if (!Tags.ContainsKey(tag))
            {
                Tags.Add(tag, 1f);
            }
        }

        public void Add(T tag, float value)
        {
            Assert.IsFalse(value <= 0, "Value must be greater than 0.");

            if (Tags.TryGetValue(tag, out float existingValue))
            {
                SetValue(tag, existingValue + value);
            }
            else
            {
                SetValue(tag, value);
            }
        }

        public void Remove(T tag)
        {
            Tags.Remove(tag);
        }

        public void Remove(T tag, float value)
        {
            Assert.IsFalse(value <= 0, "Value must be greater than 0.");

            if (Tags.TryGetValue(tag, out float existingValue))
            {
                SetValue(tag, existingValue - value);
            }
        }

        public float GetValue(T tag)
        {
            return Tags.TryGetValue(tag, out float value) ? value : 0;
        }

        public void SetValue(T tag, float value)
        {
            Assert.IsFalse(value < 0, "Value must not be negative.");

            if (Tags.ContainsKey(tag))
            {
                if (value > 0)
                {
                    Tags[tag] = value;
                }
                else
                {
                    Tags.Remove(tag);
                }
            }
            else if (value > 0)
            {
                Tags.Add(tag, value);
            }
        }

        public bool Has(T tag)
        {
            return Tags.ContainsKey(tag);
        }

        public bool Has(T tag, float threshold)
        {
            return Tags.TryGetValue(tag, out float value) && value >= threshold;
        }

        public bool HasAll(T[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Tags.ContainsKey(tags[i])) return false;
            }
            return true;
        }

        public bool HasAll(T[] tags, float threshold)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Has(tags[i], threshold)) return false;
            }
            return true;
        }

        public bool HasAny(T[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (Tags.ContainsKey(tags[i])) return true;
            }
            return false;
        }

        public bool HasAny(T[] tags, float threshold)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (Has(tags[i], threshold)) return true;
            }
            return false;
        }

        public bool Is(T tag, float value) => Tags.TryGetValue(tag, out float tagValue) && tagValue == value ? true : false;
        
        public bool Are(T[] tags, float value)
        {
            for(int i = 0; i < tags.Length; i++)
            {
                if(!Is(tags[i], value)) return false;
            }
            return true;
        }

        public bool AnyAre(T[] tags, float value)
        {
            for(int i = 0; i < tags.Length; i++)
            {
                if(Is(tags[i], value)) return true;
            }
            return false;
        }
    }
}