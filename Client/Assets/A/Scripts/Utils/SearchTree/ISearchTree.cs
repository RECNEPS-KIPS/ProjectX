using System;
using System.Collections.Generic;

namespace GameUtils
{
    public interface ISearchTree<T>
    {
        void Insert(string key, T value);
        List<T> Search(string prefix);
        void Clear();
    }

    public class TrieSearchTree<T> : ISearchTree<T>
    {
        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
            public List<T> Values { get; } = new List<T>();
            public bool IsEndOfWord { get; set; }
        }

        private readonly TrieNode root = new TrieNode();

        public void Insert(string key, T value)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            var current = root;
            foreach (var c in key.ToLower())
            {
                if (!current.Children.ContainsKey(c))
                {
                    current.Children[c] = new TrieNode();
                }
                current = current.Children[c];
                current.Values.Add(value);
            }
            current.IsEndOfWord = true;
        }

        public List<T> Search(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return new List<T>();

            var current = root;
            prefix = prefix.ToLower();
            
            foreach (var c in prefix)
            {
                if (!current.Children.ContainsKey(c))
                {
                    return new List<T>();
                }
                current = current.Children[c];
            }

            return current.Values;
        }

        public void Clear()
        {
            root.Children.Clear();
        }
    }
} 