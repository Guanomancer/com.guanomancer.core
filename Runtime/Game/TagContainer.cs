using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Guanomancer
{
    [System.Serializable]
    public class TagContainer<T> where T : TagBase
    {
        public delegate void TagEventHandler(T tag, float newValue, float oldValue);

        public event TagEventHandler TagChanged;
        public event TagEventHandler TagGained;
        public event TagEventHandler TagLost;

        public Dictionary<T, float> Tags { get; private set; } = new();

        public void Add(T tag)
        {
            if (!Tags.ContainsKey(tag))
            {
                Tags.Add(tag, 1f);
                TagGained?.Invoke(tag, 1f, 0f);
                TagChanged?.Invoke(tag, 1f, 0f);
            }
        }

        public void Add(T tag, float value)
        {
            Assert.IsFalse(value <= 0, "Value must be greater than 0.");

            if (Tags.TryGetValue(tag, out float existingValue))
            {
                SetValueSilent(tag, existingValue + value);
                TagChanged?.Invoke(tag, existingValue + value, existingValue);
            }
            else
            {
                SetValueSilent(tag, value);
                TagGained?.Invoke(tag, value, 0f);
                TagChanged?.Invoke(tag, value, 0f);
            }
        }

        public void Remove(T tag)
        {
            if (Tags.TryGetValue(tag, out float existingValue))
            {
                Tags.Remove(tag);
                TagLost?.Invoke(tag, 0f, existingValue);
                TagChanged?.Invoke(tag, 0f, existingValue);
            }
        }

        public void Remove(T tag, float value)
        {
            Assert.IsFalse(value <= 0, "Value must be greater than 0.");

            if (Tags.TryGetValue(tag, out float existingValue))
            {
                var newValue = Mathf.Max(0f, existingValue - value);
                SetValueSilent(tag, newValue);
                TagLost?.Invoke(tag, 0f, existingValue);
                TagChanged?.Invoke(tag, newValue, existingValue);
            }
        }

        public float GetValue(T tag)
        {
            return Tags.TryGetValue(tag, out float value) ? value : 0;
        }

        public void SetValue(T tag, float value)
        {
            Assert.IsFalse(value < 0, "Value must be equal to or greater than 0.");

            if (Tags.TryGetValue(tag, out float existingValue) && value != existingValue)
            {
                SetValueSilent(tag, value);
                if (value == 0) TagLost?.Invoke(tag, value, existingValue);
                TagChanged?.Invoke(tag, value, existingValue);
            }
            else if (value > 0f)
            {
                SetValueSilent(tag, value);
                TagGained(tag, value, 0f);
                TagChanged?.Invoke(tag, value, 0f);
            }
        }

        private void SetValueSilent(T tag, float value)
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
            return tag != null && Tags.ContainsKey(tag);
        }

        public bool Has(T tag, float threshold)
        {
            return tag != null && Tags.TryGetValue(tag, out float value) && value >= threshold;
        }

        public bool HasAll(T[] tags)
        {
            if (tags == null) return true;
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Tags.ContainsKey(tags[i])) return false;
            }
            return true;
        }

        public bool HasAll(T[] tags, float threshold)
        {
            if (tags == null) return true;
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Has(tags[i], threshold)) return false;
            }
            return true;
        }

        public bool HasAny(T[] tags)
        {
            if (tags == null) return false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (Tags.ContainsKey(tags[i])) return true;
            }
            return false;
        }

        public bool HasAny(T[] tags, float threshold)
        {
            if (tags == null) return false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (Has(tags[i], threshold)) return true;
            }
            return false;
        }

        public bool Equals(T tag, float value) => Tags.TryGetValue(tag, out float tagValue) && tagValue == value ? true : false;

        public bool Equals(T[] tags, float value)
        {
            if (tags == null) return false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Equals(tags[i], value)) return false;
            }
            return true;
        }

        public bool AnyEquals(T[] tags, float value)
        {
            if (tags == null) return false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (Equals(tags[i], value)) return true;
            }
            return false;
        }
    }
}