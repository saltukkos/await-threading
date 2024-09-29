// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This file has been adapted from the .NET Framework source (AsyncLocal.cs) and edited by Konstantin Saltuk.
// Date: 2024-09-29
// Changes:
// * Reimplement to use ParallelContext instead of ExecutionContext
// * Remove OnValueChanged support
// * Make it disposable and clear the data on Dispose

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwaitThreading.Core;

public sealed class ParallelLocal<T> : IParallelLocal, IDisposable
{
    [MaybeNull]
    public T Value
    {
        get
        {
            var obj = ParallelContext.GetLocalValue(this);
            return obj is null ? default : (T)obj;
        }
        set => ParallelContext.SetLocalValue(this, value);
    }

    public void Dispose()
    {
        ParallelContext.ClearLocalValue(this);
    }
}

//
// Interface to allow non-generic code in ParallelContext to call into the generic ParallelLocal<T> type.
//
internal interface IParallelLocal
{
}

//
// Interface used to store an IParallelLocal => object mapping in ParallelContext.
// Implementations are specialized based on the number of elements in the immutable
// map in order to minimize memory consumption and look-up times.
//
internal interface IParallelLocalValueMap
{
    bool TryGetValue(IParallelLocal key, out object? value);
    IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent);
}

//
// Utility functions for getting/creating instances of IParallelLocalValueMap
//
internal static class ParallelLocalValueMap
{
    public static IParallelLocalValueMap Empty { get; } = new EmptyParallelLocalValueMap();

    public static bool IsEmpty(IParallelLocalValueMap parallelLocalValueMap)
    {
        Debug.Assert(parallelLocalValueMap != null);
        Debug.Assert(parallelLocalValueMap == Empty || parallelLocalValueMap.GetType() != typeof(EmptyParallelLocalValueMap));

        return parallelLocalValueMap == Empty;
    }

    public static IParallelLocalValueMap Create(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
    {
        // If the value isn't null or a null value may not be treated as nonexistent, then create a new one-element map
        // to store the key/value pair.  Otherwise, use the empty map.
        return value != null || !treatNullValueAsNonexistent ?
            new OneElementParallelLocalValueMap(key, value) :
            Empty;
    }

    // Instance without any key/value pairs.  Used as a singleton/
    private sealed class EmptyParallelLocalValueMap : IParallelLocalValueMap
    {
        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            // If the value isn't null or a null value may not be treated as nonexistent, then create a new one-element map
            // to store the key/value pair.  Otherwise, use the empty map.
            return value != null || !treatNullValueAsNonexistent ?
                new OneElementParallelLocalValueMap(key, value) :
                this;
        }

        public bool TryGetValue(IParallelLocal key, out object? value)
        {
            value = null;
            return false;
        }
    }

    // Instance with one key/value pair.
    private sealed class OneElementParallelLocalValueMap : IParallelLocalValueMap
    {
        private readonly IParallelLocal _key1;
        private readonly object? _value1;

        public OneElementParallelLocalValueMap(IParallelLocal key, object? value)
        {
            _key1 = key; _value1 = value;
        }

        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            if (value != null || !treatNullValueAsNonexistent)
            {
                // If the key matches one already contained in this map, then create a new one-element map with the updated
                // value, otherwise create a two-element map with the additional key/value.
                return ReferenceEquals(key, _key1) ?
                    new OneElementParallelLocalValueMap(key, value) :
                    new TwoElementParallelLocalValueMap(_key1, _value1, key, value);
            }

            // If the key exists in this map, remove it by downgrading to an empty map.  Otherwise, there's nothing to
            // add or remove, so just return this map.
            return ReferenceEquals(key, _key1) ?
                Empty :
                this;
        }

        public bool TryGetValue(IParallelLocal key, out object? value)
        {
            if (ReferenceEquals(key, _key1))
            {
                value = _value1;
                return true;
            }

            value = null;
            return false;
        }
    }

    // Instance with two key/value pairs.
    private sealed class TwoElementParallelLocalValueMap : IParallelLocalValueMap
    {
        private readonly IParallelLocal _key1, _key2;
        private readonly object? _value1, _value2;

        public TwoElementParallelLocalValueMap(IParallelLocal key1, object? value1, IParallelLocal key2, object? value2)
        {
            _key1 = key1; _value1 = value1;
            _key2 = key2; _value2 = value2;
        }

        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            if (value != null || !treatNullValueAsNonexistent)
            {
                // If the key matches one already contained in this map, then create a new two-element map with the updated
                // value, otherwise create a three-element map with the additional key/value.
                return
                    ReferenceEquals(key, _key1) ? new TwoElementParallelLocalValueMap(key, value, _key2, _value2) :
                    ReferenceEquals(key, _key2) ? new TwoElementParallelLocalValueMap(_key1, _value1, key, value) :
                    new ThreeElementParallelLocalValueMap(_key1, _value1, _key2, _value2, key, value);
            }

            // If the key exists in this map, remove it by downgrading to a one-element map without the key.  Otherwise,
            // there's nothing to add or remove, so just return this map.
            return
                ReferenceEquals(key, _key1) ? new OneElementParallelLocalValueMap(_key2, _value2) :
                ReferenceEquals(key, _key2) ? new OneElementParallelLocalValueMap(_key1, _value1) :
                this;
        }

        public bool TryGetValue(IParallelLocal key, out object? value)
        {
            if (ReferenceEquals(key, _key1))
            {
                value = _value1;
                return true;
            }

            if (ReferenceEquals(key, _key2))
            {
                value = _value2;
                return true;
            }

            value = null;
            return false;
        }
    }

    // Instance with three key/value pairs.
    private sealed class ThreeElementParallelLocalValueMap : IParallelLocalValueMap
    {
        private readonly IParallelLocal _key1, _key2, _key3;
        private readonly object? _value1, _value2, _value3;

        public ThreeElementParallelLocalValueMap(IParallelLocal key1, object? value1, IParallelLocal key2, object? value2, IParallelLocal key3, object? value3)
        {
            _key1 = key1; _value1 = value1;
            _key2 = key2; _value2 = value2;
            _key3 = key3; _value3 = value3;
        }

        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            if (value != null || !treatNullValueAsNonexistent)
            {
                // If the key matches one already contained in this map, then create a new three-element map with the
                // updated value.
                if (ReferenceEquals(key, _key1)) return new ThreeElementParallelLocalValueMap(key, value, _key2, _value2, _key3, _value3);
                if (ReferenceEquals(key, _key2)) return new ThreeElementParallelLocalValueMap(_key1, _value1, key, value, _key3, _value3);
                if (ReferenceEquals(key, _key3)) return new ThreeElementParallelLocalValueMap(_key1, _value1, _key2, _value2, key, value);

                // The key doesn't exist in this map, so upgrade to a multi map that contains
                // the additional key/value pair.
                var multi = new MultiElementParallelLocalValueMap(4);
                multi.UnsafeStore(0, _key1, _value1);
                multi.UnsafeStore(1, _key2, _value2);
                multi.UnsafeStore(2, _key3, _value3);
                multi.UnsafeStore(3, key, value);
                return multi;
            }

            // If the key exists in this map, remove it by downgrading to a two-element map without the key.  Otherwise,
            // there's nothing to add or remove, so just return this map.
            return
                ReferenceEquals(key, _key1) ? new TwoElementParallelLocalValueMap(_key2, _value2, _key3, _value3) :
                ReferenceEquals(key, _key2) ? new TwoElementParallelLocalValueMap(_key1, _value1, _key3, _value3) :
                ReferenceEquals(key, _key3) ? new TwoElementParallelLocalValueMap(_key1, _value1, _key2, _value2) :
                this;
        }

        public bool TryGetValue(IParallelLocal key, out object? value)
        {
            if (ReferenceEquals(key, _key1))
            {
                value = _value1;
                return true;
            }

            if (ReferenceEquals(key, _key2))
            {
                value = _value2;
                return true;
            }

            if (ReferenceEquals(key, _key3))
            {
                value = _value3;
                return true;
            }

            value = null;
            return false;
        }
    }

    // Instance with up to 16 key/value pairs.
    private sealed class MultiElementParallelLocalValueMap : IParallelLocalValueMap
    {
        internal const int MaxMultiElements = 16;
        private readonly KeyValuePair<IParallelLocal, object?>[] _keyValues;

        internal MultiElementParallelLocalValueMap(int count)
        {
            Debug.Assert(count <= MaxMultiElements);
            _keyValues = new KeyValuePair<IParallelLocal, object?>[count];
        }

        internal void UnsafeStore(int index, IParallelLocal key, object? value)
        {
            Debug.Assert(index < _keyValues.Length);
            _keyValues[index] = new KeyValuePair<IParallelLocal, object?>(key, value);
        }

        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            // Find the key in this map.
            for (var i = 0; i < _keyValues.Length; i++)
            {
                if (ReferenceEquals(key, _keyValues[i].Key))
                {
                    // The key is in the map.
                    if (value != null || !treatNullValueAsNonexistent)
                    {
                        // Create a new map of the same size that has all of the same pairs, with this new key/value pair
                        // overwriting the old.
                        var multi = new MultiElementParallelLocalValueMap(_keyValues.Length);
                        Array.Copy(_keyValues, multi._keyValues, _keyValues.Length);
                        multi._keyValues[i] = new KeyValuePair<IParallelLocal, object?>(key, value);
                        return multi;
                    }

                    if (_keyValues.Length == 4)
                    {
                        // We only have four elements, one of which we're removing, so downgrade to a three-element map,
                        // without the matching element.
                        return
                            i == 0 ? new ThreeElementParallelLocalValueMap(_keyValues[1].Key, _keyValues[1].Value, _keyValues[2].Key, _keyValues[2].Value, _keyValues[3].Key, _keyValues[3].Value) :
                            i == 1 ? new ThreeElementParallelLocalValueMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[2].Key, _keyValues[2].Value, _keyValues[3].Key, _keyValues[3].Value) :
                            i == 2 ? new ThreeElementParallelLocalValueMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[1].Key, _keyValues[1].Value, _keyValues[3].Key, _keyValues[3].Value) :
                            (IParallelLocalValueMap)new ThreeElementParallelLocalValueMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[1].Key, _keyValues[1].Value, _keyValues[2].Key, _keyValues[2].Value);
                    }

                    {
                        // We have enough elements remaining to warrant a multi map.  Create a new one and copy all of the
                        // elements from this one, except the one to be removed.
                        var multi = new MultiElementParallelLocalValueMap(_keyValues.Length - 1);
                        if (i != 0) Array.Copy(_keyValues, multi._keyValues, i);
                        if (i != _keyValues.Length - 1) Array.Copy(_keyValues, i + 1, multi._keyValues, i, _keyValues.Length - i - 1);
                        return multi;
                    }
                }
            }

            // The key does not already exist in this map.

            if (value == null && treatNullValueAsNonexistent)
            {
                // We can simply return this same map, as there's nothing to add or remove.
                return this;
            }

            // We need to create a new map that has the additional key/value pair.
            // If with the addition we can still fit in a multi map, create one.
            if (_keyValues.Length < MaxMultiElements)
            {
                var multi = new MultiElementParallelLocalValueMap(_keyValues.Length + 1);
                Array.Copy(_keyValues, multi._keyValues, _keyValues.Length);
                multi._keyValues[_keyValues.Length] = new KeyValuePair<IParallelLocal, object?>(key, value);
                return multi;
            }

            // Otherwise, upgrade to a many map.
            var many = new ManyElementParallelLocalValueMap(MaxMultiElements + 1);
            foreach (var pair in _keyValues)
            {
                many[pair.Key] = pair.Value;
            }
            many[key] = value;
            return many;
        }

        public bool TryGetValue(IParallelLocal key, out object? value)
        {
            foreach (var pair in _keyValues)
            {
                if (ReferenceEquals(key, pair.Key))
                {
                    value = pair.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }

    // Instance with any number of key/value pairs.
    private sealed class ManyElementParallelLocalValueMap : Dictionary<IParallelLocal, object?>, IParallelLocalValueMap
    {
        public ManyElementParallelLocalValueMap(int capacity) : base(capacity) { }

        public IParallelLocalValueMap Set(IParallelLocal key, object? value, bool treatNullValueAsNonexistent)
        {
            var count = Count;
            var containsKey = ContainsKey(key);

            // If the value being set exists, create a new many map, copy all of the elements from this one,
            // and then store the new key/value pair into it.  This is the most common case.
            if (value != null || !treatNullValueAsNonexistent)
            {
                var map = new ManyElementParallelLocalValueMap(count + (containsKey ? 0 : 1));
                foreach (var pair in this)
                {
                    map[pair.Key] = pair.Value;
                }
                map[key] = value;
                return map;
            }

            // Otherwise, the value is null and a null value may be treated as nonexistent. We can downgrade to a smaller
            // map rather than storing null.

            // If the key is contained in this map, we're going to create a new map that's one pair smaller.
            if (containsKey)
            {
                // If the new count would be within range of a multi map instead of a many map,
                // downgrade to the multi map, which uses less memory and is faster to access.
                // Otherwise, just create a new many map that's missing this key.
                if (count == MultiElementParallelLocalValueMap.MaxMultiElements + 1)
                {
                    var multi = new MultiElementParallelLocalValueMap(MultiElementParallelLocalValueMap.MaxMultiElements);
                    var index = 0;
                    foreach (var pair in this)
                    {
                        if (!ReferenceEquals(key, pair.Key))
                        {
                            multi.UnsafeStore(index++, pair.Key, pair.Value);
                        }
                    }
                    Debug.Assert(index == MultiElementParallelLocalValueMap.MaxMultiElements);
                    return multi;
                }

                var map = new ManyElementParallelLocalValueMap(count - 1);
                foreach (var pair in this)
                {
                    if (!ReferenceEquals(key, pair.Key))
                    {
                        map[pair.Key] = pair.Value;
                    }
                }
                Debug.Assert(map.Count == count - 1);
                return map;
            }

            // We were storing null and a null value may be treated as nonexistent, but the key wasn't in the map, so
            // there's nothing to change.  Just return this instance.
            return this;
        }
    }
}