// author:KIPKIPS
// date:2024.10.26 18:20
// describe:
using UnityEngine;
using System.Collections.Generic;

namespace Framework.Core.SpaceSegment
{
    public abstract class LinearTree<T> : ITree<T> where T : IScenable, IScenableLinkedListNode
    {
        public Bounds Bounds => m_Bounds;
        public int MaxDepth => m_MaxDepth;
        protected int m_MaxDepth;
        protected Bounds m_Bounds;
        protected Dictionary<uint, LinearTreeLeaf<T>> m_Nodes; //使用Morton码索引的节点字典
        protected int m_Cols;
        public LinearTree(Vector3 center, Vector3 size, int maxDepth)
        {
            m_MaxDepth = maxDepth;
            m_Bounds = new Bounds(center, size);
            m_Cols = (int)Mathf.Pow(2, maxDepth);
            m_Nodes = new Dictionary<uint, LinearTreeLeaf<T>>();
        }
        public void Clear()
        {
            m_Nodes.Clear();
        }
        public bool Contains(T item)
        {
            if (m_Nodes == null)
                return false;
            foreach (var node in m_Nodes)
            {
                if (node.Value != null && node.Value.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
        public void Remove(T item)
        {
            if (item == null || m_Nodes == null)
            {
                return;
            }
            var nodes = item.GetNodes();
            if (nodes == null)
            {
                return;
            }
            foreach (var node in nodes)
            {
                if (m_Nodes.ContainsKey(node.Key))
                {
                    var n = m_Nodes[node.Key];
                    if (n != null && n.Datas != null)
                    {
                        var value = (LinkedListNode<T>)node.Value;
                        if (value.List == n.Datas)
                        {
                            n.Datas.Remove(value);
                        }
                    }
                }
            }
            nodes.Clear();
        }
        public abstract void Add(T item);
        public abstract void Trigger(IDetector detector, TriggerHandle<T> handle);
#if UNITY_EDITOR
        public abstract void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj);
#endif
    }
    public class LinearTreeLeaf<T> where T : IScenable, IScenableLinkedListNode
    {
        public LinkedList<T> Datas => m_DataList;
        private LinkedList<T> m_DataList;
        public LinearTreeLeaf()
        {
            m_DataList = new LinkedList<T>();
        }
        public LinkedListNode<T> Insert(T obj)
        {
            return m_DataList.AddFirst(obj);
        }
        public void Trigger(IDetector detector, TriggerHandle<T> handle)
        {
            if (handle != null)
            {
                LinkedListNode<T> node = m_DataList.First;
                while (node != null)
                {
                    if (detector.IsDetected(node.Value.Bounds))
                    {
                        handle(node.Value);
                    }
                    node = node.Next;
                }
            }
        }
        public bool Contains(T item)
        {
            return m_DataList != null && m_DataList.Contains(item);
        }
#if UNITY_EDITOR
        public bool DrawNode(Color objColor, Color hitObjColor, bool drawObj)
        {
            if (drawObj && m_DataList.Count > 0)
            {
                LinkedListNode<T> node = m_DataList.First;
                while (node != null)
                {
                    if (node.Value is Scenable sceneObj)
                    {
                        sceneObj.DrawArea(objColor, hitObjColor);
                    }
                    node = node.Next;
                }
                return true;
            }
            return false;
        }
#endif
    }
}