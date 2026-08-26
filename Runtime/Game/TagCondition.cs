using System;
using UnityEngine;

namespace Guanomancer
{
    public interface ITagCondition<T> where T : TagBase
    {
        bool Evaluate(TagContainer<T> container);
    }

    public enum TagConditionOperator
    {
        // Single Tag
        HasTag,
        DoesNotHaveTag,
        ValueEquals,
        ValueGreaterThan,
        ValueLessThan,
        ValueGreaterThanOrEqual,
        ValueLessThanOrEqual,

        // Multiple Tags (Flat Array)
        HasAllTags,
        HasAnyTags,
        HasNoneOfTags,

        // Hierarchical Composite Conditions
        AllOf,   // AND
        AnyOf,   // OR
        NoneOf   // NOR
    }

    [Serializable]
    public class TagCondition<T> : ITagCondition<T> where T : TagBase
    {
        [SerializeField] private TagConditionOperator _operator = TagConditionOperator.HasTag;
        [SerializeField] private bool _invert = false;

        // Single Tag operands
        [SerializeField] private T _tag;
        [SerializeField] private float _value = 1f;

        // Multiple Tags operands
        [SerializeField] private T[] _tags;
        [SerializeField] private float _threshold = 1f;

        // Hierarchical Sub-Conditions (uses SerializeReference to prevent recursion depth warnings)
        [SerializeReference] private TagCondition<T>[] _subConditions;

        public TagConditionOperator Operator
        {
            get => _operator;
            set => _operator = value;
        }

        public bool Invert
        {
            get => _invert;
            set => _invert = value;
        }

        public T Tag
        {
            get => _tag;
            set => _tag = value;
        }

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public T[] Tags
        {
            get => _tags;
            set => _tags = value;
        }

        public float Threshold
        {
            get => _threshold;
            set => _threshold = value;
        }

        public TagCondition<T>[] SubConditions
        {
            get => _subConditions;
            set => _subConditions = value;
        }

        public TagCondition() { }

        public TagCondition(TagConditionOperator op, T tag, float value = 1f, bool invert = false)
        {
            _operator = op;
            _tag = tag;
            _value = value;
            _invert = invert;
        }

        public TagCondition(TagConditionOperator op, T[] tags, float threshold = 1f, bool invert = false)
        {
            _operator = op;
            _tags = tags;
            _threshold = threshold;
            _invert = invert;
        }

        public TagCondition(TagConditionOperator op, TagCondition<T>[] subConditions, bool invert = false)
        {
            _operator = op;
            _subConditions = subConditions;
            _invert = invert;
        }

        public bool Evaluate(TagContainer<T> container)
        {
            if (container == null) return false;

            bool result = _operator switch
            {
                TagConditionOperator.HasTag => _tag != null && container.Has(_tag),
                TagConditionOperator.DoesNotHaveTag => _tag == null || !container.Has(_tag),
                TagConditionOperator.ValueEquals => _tag != null && container.Equals(_tag, _value),
                TagConditionOperator.ValueGreaterThan => _tag != null && container.GetValue(_tag) > _value,
                TagConditionOperator.ValueLessThan => _tag != null && container.GetValue(_tag) < _value,
                TagConditionOperator.ValueGreaterThanOrEqual => _tag != null && container.GetValue(_tag) >= _value,
                TagConditionOperator.ValueLessThanOrEqual => _tag != null && container.GetValue(_tag) <= _value,

                TagConditionOperator.HasAllTags => _tags != null && container.HasAll(_tags, _threshold),
                TagConditionOperator.HasAnyTags => _tags != null && container.HasAny(_tags, _threshold),
                TagConditionOperator.HasNoneOfTags => _tags == null || !container.HasAny(_tags, _threshold),

                TagConditionOperator.AllOf => EvaluateAll(container),
                TagConditionOperator.AnyOf => EvaluateAny(container),
                TagConditionOperator.NoneOf => !EvaluateAny(container),

                _ => false
            };

            return _invert ? !result : result;
        }

        private bool EvaluateAll(TagContainer<T> container)
        {
            if (_subConditions == null || _subConditions.Length == 0) return true;
            for (int i = 0; i < _subConditions.Length; i++)
            {
                if (_subConditions[i] != null && !_subConditions[i].Evaluate(container))
                    return false;
            }
            return true;
        }

        private bool EvaluateAny(TagContainer<T> container)
        {
            if (_subConditions == null || _subConditions.Length == 0) return false;
            for (int i = 0; i < _subConditions.Length; i++)
            {
                if (_subConditions[i] != null && _subConditions[i].Evaluate(container))
                    return true;
            }
            return false;
        }
    }
}
