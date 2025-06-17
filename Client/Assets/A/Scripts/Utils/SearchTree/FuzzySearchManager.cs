using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUtils
{
    /// <summary>
    /// 搜索维度配置
    /// </summary>
    /// <typeparam name="T">要搜索的对象类型</typeparam>
    public class SearchDimension<T>
    {
        public string DimensionName { get; }
        public Func<T, string> KeySelector { get; }

        public SearchDimension(string dimensionName, Func<T, string> keySelector)
        {
            DimensionName = dimensionName;
            KeySelector = keySelector;
        }
    }

    /// <summary>
    /// 泛型模糊搜索管理器
    /// </summary>
    /// <typeparam name="T">要搜索的对象类型</typeparam>
    public class FuzzySearchManager<T>
    {
        private readonly Dictionary<string, ISearchTree<T>> searchTrees;
        private readonly List<SearchDimension<T>> searchDimensions;

        public FuzzySearchManager()
        {
            searchTrees = new Dictionary<string, ISearchTree<T>>();
            searchDimensions = new List<SearchDimension<T>>();
        }

        /// <summary>
        /// 添加搜索维度
        /// </summary>
        /// <param name="dimensionName">维度名称</param>
        /// <param name="keySelector">键选择器</param>
        public void AddSearchDimension(string dimensionName, Func<T, string> keySelector)
        {
            if (string.IsNullOrEmpty(dimensionName))
                throw new ArgumentException("维度名称不能为空", nameof(dimensionName));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (searchTrees.ContainsKey(dimensionName))
                throw new ArgumentException($"维度 '{dimensionName}' 已存在", nameof(dimensionName));

            var dimension = new SearchDimension<T>(dimensionName, keySelector);
            searchDimensions.Add(dimension);
            searchTrees[dimensionName] = new TrieSearchTree<T>();
        }

        /// <summary>
        /// 索引单个对象
        /// </summary>
        public void IndexObject(T obj)
        {
            if (obj == null) return;

            foreach (var dimension in searchDimensions)
            {
                var key = dimension.KeySelector(obj);
                if (!string.IsNullOrEmpty(key))
                {
                    searchTrees[dimension.DimensionName].Insert(key, obj);
                }
            }
        }

        /// <summary>
        /// 批量索引对象
        /// </summary>
        public void IndexObjects(IEnumerable<T> objects)
        {
            if (objects == null) return;
            foreach (var obj in objects)
            {
                IndexObject(obj);
            }
        }

        /// <summary>
        /// 按维度搜索
        /// </summary>
        public List<T> SearchByDimension(string dimensionName, string prefix)
        {
            if (!searchTrees.ContainsKey(dimensionName))
                throw new ArgumentException($"未找到维度 '{dimensionName}'", nameof(dimensionName));

            return searchTrees[dimensionName].Search(prefix);
        }

        /// <summary>
        /// 获取所有可用的搜索维度名称
        /// </summary>
        public IEnumerable<string> GetAvailableDimensions()
        {
            return searchDimensions.Select(d => d.DimensionName);
        }

        /// <summary>
        /// 清除所有索引
        /// </summary>
        public void Clear()
        {
            foreach (var tree in searchTrees.Values)
            {
                tree.Clear();
            }
        }

        /// <summary>
        /// 移除搜索维度
        /// </summary>
        public bool RemoveSearchDimension(string dimensionName)
        {
            if (!searchTrees.ContainsKey(dimensionName))
                return false;

            searchTrees.Remove(dimensionName);
            searchDimensions.RemoveAll(d => d.DimensionName == dimensionName);
            return true;
        }
    }
} 