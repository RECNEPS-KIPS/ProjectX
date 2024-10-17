// author:KIPKIPS
// date:2022.05.24 18:05
// describe:可序列化字典实现

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Framework.Core.Container
{
    /// <summary>
    /// 可序列化字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        // OnAfterDeserialize implementation
        /// <summary>
        /// 序列化完成后
        /// </summary>
        public void OnAfterDeserialize()
        {
            for (var i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }

            keys.Clear();
            values.Clear();
        }
    }
}