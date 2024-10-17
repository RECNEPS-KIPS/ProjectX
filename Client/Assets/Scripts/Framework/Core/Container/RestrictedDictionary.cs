// author:KIPKIPS
// date:2022.05.24 18:05
// describe:只读字典,开启读权限

using System.Collections.Generic;

namespace Framework.Core.Container
{
    /// <summary>
    /// 只读字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class RestrictedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private bool _writable;

        /// <summary>
        /// 开启可写
        /// </summary>
        public void EnableWrite()
        {
            _writable = true;
        }

        /// <summary>
        /// 禁止读操作
        /// </summary>
        public void ForbidWrite()
        {
            _writable = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RestrictedDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        private IDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (_writable)
            {
                _dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// 包含Key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Keys列表
        /// </summary>
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <summary>
        /// 移除key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            return _writable && _dictionary.Remove(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Values列表
        /// </summary>
        public ICollection<TValue> Values => _dictionary.Values;

        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="key"></param>
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_writable)
                {
                    _dictionary[key] = value;
                }
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set
            {
                if (_writable)
                {
                    this[key] = value;
                }
            }
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (_writable)
            {
                _dictionary.Add(item);
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            if (_writable)
            {
                _dictionary.Clear();
            }
        }

        /// <summary>
        /// 是否包含键值对
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 当前容量
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 移除键值对
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _writable && _dictionary.Remove(item);
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}