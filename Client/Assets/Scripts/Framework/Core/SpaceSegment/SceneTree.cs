// author:KIPKIPS
// date:2024.10.26 18:21
// describe:
using UnityEngine;
using System.Collections.Generic;
using Framework.Common;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 场景树（非线性结构）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SceneTree<T> : ITree<T> where T : IScenable, IScenableLinkedListNode
    {
        public Bounds Bounds => root?.Bounds ?? default(Bounds);
        public int MaxDepth => maxDepth;
        /// <summary>
        /// 最大深度
        /// </summary>
        protected readonly int maxDepth;
        protected readonly SceneTreeNode<T> root;
        public SceneTree(Vector3 center, Vector3 size, int maxDepth, bool ocTree)
        {
            this.maxDepth = maxDepth;
            this.root = new SceneTreeNode<T>(new Bounds(center, size), 0, ocTree ? 8 : 4);
        }
        public void Add(T item)
        {
            root.Insert(item, 0, maxDepth);
        }
        public void Clear()
        {
            root.Clear();
        }
        public bool Contains(T item)
        {
            return root.Contains(item);
        }
        public void Remove(T item)
        {
            root.Remove(item);
        }
        public void Trigger(IDetector detector, TriggerHandle<T> handle)
        {
            if (handle == null)
                return;
            if (detector.UseCameraCulling)
            {
                root.Trigger(detector, handle);
            } else
            {
                if (detector.IsDetected(Bounds) == false)
                    return;
                root.Trigger(detector, handle);
            }
        }
        public static implicit operator bool(SceneTree<T> tree)
        {
            return tree != null;
        }
#if UNITY_EDITOR
        public void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj)
        {
            root?.DrawNode(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, maxDepth);
        }
#endif
    }
    public class SceneTreeNode<T> where T : IScenable, IScenableLinkedListNode
    {
        public Bounds Bounds => m_Bounds;
        /// <summary>
        /// 节点当前深度
        /// </summary>
        public int CurrentDepth => m_CurrentDepth;
        /// <summary>
        /// 节点数据列表
        /// </summary>
        public LinkedList<T> ObjectList => m_ObjectList;
        private int m_CurrentDepth;
        private Vector3 m_HalfSize;
        private LinkedList<T> m_ObjectList;
        private SceneTreeNode<T>[] m_ChildNodes;
        private int m_ChildCount;
        private Bounds m_Bounds;
        public SceneTreeNode(Bounds bounds, int depth, int childCount)
        {
            m_Bounds = bounds;
            m_CurrentDepth = depth;
            m_ObjectList = new LinkedList<T>();
            m_ChildNodes = new SceneTreeNode<T>[childCount];
            m_HalfSize = childCount == 8 ? new Vector3(m_Bounds.size.x / 2, m_Bounds.size.y / 2, m_Bounds.size.z / 2) : new Vector3(m_Bounds.size.x / 2, m_Bounds.size.y, m_Bounds.size.z / 2);
            m_ChildCount = childCount;
        }
        public void Clear()
        {
            foreach (var t in m_ChildNodes)
            {
                t?.Clear();
            }
            m_ObjectList?.Clear();
        }
        public bool Contains(T obj)
        {
            foreach (var t in m_ChildNodes)
            {
                if (t != null && t.Contains(obj))
                {
                    return true;
                }
            }
            return m_ObjectList != null && m_ObjectList.Contains(obj);
        }
        public SceneTreeNode<T> Insert(T obj, int depth, int maxDepth)
        {
            if (m_ObjectList.Contains(obj))
            {
                return this;
            }
            if (depth < maxDepth)
            {
                SceneTreeNode<T> node = GetContainerNode(obj, depth);
                if (node != null)
                {
                    return node.Insert(obj, depth + 1, maxDepth);
                }
            }
            var n = m_ObjectList.AddFirst(obj);
            obj.SetLinkedListNode(0, n);
            return this;
        }
        public void Remove(T obj)
        {
            var node = obj.GetLinkedListNode<T>(0);
            if (node != null)
            {
                if (node.List == m_ObjectList)
                {
                    m_ObjectList.Remove(node);
                    var nodes = obj.GetNodes();
                    nodes?.Clear();
                    return;
                }
            }
            if (m_ChildNodes == null || m_ChildNodes.Length <= 0) return;
            foreach (var t in m_ChildNodes)
            {
                t?.Remove(obj);
            }
            //{
            //    return true;
            //}
            //return false;
        }
        public void Trigger(IDetector detector, TriggerHandle<T> handle)
        {
            if (handle == null)
            {
                return;
            }
            if (detector.UseCameraCulling)
            {
                TreeCullingCode code = new TreeCullingCode()
                {
                    leftBottomBack = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.min.y, m_Bounds.min.z, true),
                    leftBottomForward = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.min.y, m_Bounds.max.z, true),
                    leftTopBack = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.min.z, true),
                    leftTopForward = detector.GetDetectedCode(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.max.z, true),
                    rightBottomBack = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.min.z, true),
                    rightBottomForward = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.max.z, true),
                    rightTopBack = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.max.y, m_Bounds.min.z, true),
                    rightTopForward = detector.GetDetectedCode(m_Bounds.max.x, m_Bounds.max.y, m_Bounds.max.z, true),
                };
                TriggerByCamera(detector, handle, code);
            } else
            {
                int code = detector.GetDetectedCode(m_Bounds.center.x, m_Bounds.center.y, m_Bounds.center.z, m_ChildCount == 4);
                for (int i = 0; i < m_ChildNodes.Length; i++)
                {
                    var node = m_ChildNodes[i];
                    if (node != null && (code & (1 << i)) != 0)
                    {
                        node.Trigger(detector, handle);
                    }
                }
                {
                    var node = m_ObjectList.First;
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
        }
        private void TriggerByCamera(IDetector detector, TriggerHandle<T> handle, TreeCullingCode code)
        {
            if (code.IsCulled())
                return;
            var node = m_ObjectList.First;
            while (node != null)
            {
                if (detector.IsDetected(node.Value.Bounds))
                    handle(node.Value);
                node = node.Next;
            }
            float centerX = m_Bounds.center.x, centerY = m_Bounds.center.y, centerZ = m_Bounds.center.z;
            float sx = m_Bounds.size.x * 0.5f, sy = m_Bounds.size.y * 0.5f, sz = m_Bounds.size.z * 0.5f;
            int leftBottomMiddle = detector.GetDetectedCode(centerX - sx, centerY - sy, centerZ, true);
            int middleBottomMiddle = detector.GetDetectedCode(centerX, centerY - sy, centerZ, true);
            int rightBottomMiddle = detector.GetDetectedCode(centerX + sx, centerY - sy, centerZ, true);
            int middleBottomBack = detector.GetDetectedCode(centerX, centerY - sy, centerZ - sz, true);
            int middleBottomForward = detector.GetDetectedCode(centerX, centerY - sy, centerZ + sz, true);
            int leftTopMiddle = detector.GetDetectedCode(centerX - sx, centerY + sy, centerZ, true);
            int middleTopMiddle = detector.GetDetectedCode(centerX, centerY + sy, centerZ, true);
            int rightTopMiddle = detector.GetDetectedCode(centerX + sx, centerY + sy, centerZ, true);
            int middleTopBack = detector.GetDetectedCode(centerX, centerY + sy, centerZ - sz, true);
            int middleTopForward = detector.GetDetectedCode(centerX, centerY + sy, centerZ + sz, true);
            if (m_ChildCount == 8)
            {
                int leftMiddleBack = detector.GetDetectedCode(centerX - sx, centerY, centerZ - sz, true);
                int leftMiddleMiddle = detector.GetDetectedCode(centerX - sx, centerY, centerZ, true);
                int leftMiddleForward = detector.GetDetectedCode(centerX - sx, centerY, centerZ + sz, true);
                int middleMiddleBack = detector.GetDetectedCode(centerX, centerY, centerZ - sz, true);
                int middleMiddleMiddle = detector.GetDetectedCode(centerX, centerY, centerZ, true);
                int middleMiddleForward = detector.GetDetectedCode(centerX, centerY, centerZ + sz, true);
                int rightMiddleBack = detector.GetDetectedCode(centerX + sx, centerY, centerZ - sz, true);
                int rightMiddleMiddle = detector.GetDetectedCode(centerX + sx, centerY, centerZ, true);
                int rightMiddleForward = detector.GetDetectedCode(centerX + sx, centerY, centerZ + sz, true);
                if (m_ChildNodes.Length > 0 && m_ChildNodes[0] != null)
                    m_ChildNodes[0].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = code.leftBottomBack,
                        leftBottomForward = leftBottomMiddle,
                        leftTopBack = leftMiddleBack,
                        leftTopForward = leftMiddleMiddle,
                        rightBottomBack = middleBottomBack,
                        rightBottomForward = middleBottomMiddle,
                        rightTopBack = middleMiddleBack,
                        rightTopForward = middleMiddleMiddle,
                    });
                if (m_ChildNodes.Length > 1 && m_ChildNodes[1] != null)
                    m_ChildNodes[1].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = leftBottomMiddle,
                        leftBottomForward = code.leftBottomForward,
                        leftTopBack = leftMiddleMiddle,
                        leftTopForward = leftMiddleForward,
                        rightBottomBack = middleBottomMiddle,
                        rightBottomForward = middleBottomForward,
                        rightTopBack = middleMiddleMiddle,
                        rightTopForward = middleMiddleForward,
                    });
                if (m_ChildNodes.Length > 2 && m_ChildNodes[2] != null)
                    m_ChildNodes[2].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = leftMiddleBack,
                        leftBottomForward = leftMiddleMiddle,
                        leftTopBack = code.leftTopBack,
                        leftTopForward = leftTopMiddle,
                        rightBottomBack = middleMiddleBack,
                        rightBottomForward = middleMiddleMiddle,
                        rightTopBack = middleTopBack,
                        rightTopForward = middleTopMiddle,
                    });
                if (m_ChildNodes.Length > 3 && m_ChildNodes[3] != null)
                    m_ChildNodes[3].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = leftMiddleMiddle,
                        leftBottomForward = leftMiddleForward,
                        leftTopBack = leftTopMiddle,
                        leftTopForward = code.leftTopForward,
                        rightBottomBack = middleMiddleMiddle,
                        rightBottomForward = middleMiddleForward,
                        rightTopBack = middleTopMiddle,
                        rightTopForward = middleTopForward,
                    });
                if (m_ChildNodes.Length > 4 && m_ChildNodes[4] != null)
                    m_ChildNodes[4].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomBack,
                        leftBottomForward = middleBottomMiddle,
                        leftTopBack = middleMiddleBack,
                        leftTopForward = middleMiddleMiddle,
                        rightBottomBack = code.rightBottomBack,
                        rightBottomForward = rightBottomMiddle,
                        rightTopBack = rightMiddleBack,
                        rightTopForward = rightMiddleMiddle,
                    });
                if (m_ChildNodes.Length > 5 && m_ChildNodes[5] != null)
                    m_ChildNodes[5].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomMiddle,
                        leftBottomForward = middleBottomForward,
                        leftTopBack = middleMiddleMiddle,
                        leftTopForward = middleMiddleForward,
                        rightBottomBack = rightBottomMiddle,
                        rightBottomForward = code.rightBottomForward,
                        rightTopBack = rightMiddleMiddle,
                        rightTopForward = rightMiddleForward,
                    });
                if (m_ChildNodes.Length > 6 && m_ChildNodes[6] != null)
                    m_ChildNodes[6].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleMiddleBack,
                        leftBottomForward = middleMiddleMiddle,
                        leftTopBack = middleTopBack,
                        leftTopForward = middleTopMiddle,
                        rightBottomBack = rightMiddleBack,
                        rightBottomForward = rightMiddleMiddle,
                        rightTopBack = code.rightTopBack,
                        rightTopForward = rightTopMiddle,
                    });
                if (m_ChildNodes.Length > 7 && m_ChildNodes[7] != null)
                    m_ChildNodes[7].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleMiddleMiddle,
                        leftBottomForward = middleMiddleForward,
                        leftTopBack = middleTopMiddle,
                        leftTopForward = middleTopForward,
                        rightBottomBack = rightMiddleMiddle,
                        rightBottomForward = rightMiddleForward,
                        rightTopBack = rightTopMiddle,
                        rightTopForward = code.rightTopForward,
                    });
            } else
            {
                if (m_ChildNodes.Length > 0 && m_ChildNodes[0] != null)
                    m_ChildNodes[0].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = code.leftBottomBack,
                        leftBottomForward = leftBottomMiddle,
                        leftTopBack = code.leftTopBack,
                        leftTopForward = leftTopMiddle,
                        rightBottomBack = middleBottomBack,
                        rightBottomForward = middleBottomMiddle,
                        rightTopBack = middleTopBack,
                        rightTopForward = middleTopMiddle,
                    });
                if (m_ChildNodes.Length > 1 && m_ChildNodes[1] != null)
                    m_ChildNodes[1].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = leftBottomMiddle,
                        leftBottomForward = code.leftBottomForward,
                        leftTopBack = leftTopMiddle,
                        leftTopForward = code.leftTopForward,
                        rightBottomBack = middleBottomMiddle,
                        rightBottomForward = middleBottomForward,
                        rightTopBack = middleTopMiddle,
                        rightTopForward = middleTopForward,
                    });
                if (m_ChildNodes.Length > 2 && m_ChildNodes[2] != null)
                    m_ChildNodes[2].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomBack,
                        leftBottomForward = middleBottomMiddle,
                        leftTopBack = middleTopBack,
                        leftTopForward = middleTopMiddle,
                        rightBottomBack = code.rightBottomBack,
                        rightBottomForward = rightBottomMiddle,
                        rightTopBack = code.rightTopBack,
                        rightTopForward = rightTopMiddle,
                    });
                if (m_ChildNodes.Length > 3 && m_ChildNodes[3] != null)
                    m_ChildNodes[3].TriggerByCamera(detector, handle, new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomMiddle,
                        leftBottomForward = middleBottomForward,
                        leftTopBack = middleTopMiddle,
                        leftTopForward = middleTopForward,
                        rightBottomBack = rightBottomMiddle,
                        rightBottomForward = code.rightBottomForward,
                        rightTopBack = rightTopMiddle,
                        rightTopForward = code.rightTopForward,
                    });
            }
        }
        protected SceneTreeNode<T> GetContainerNode(T obj, int depth)
        {
            SceneTreeNode<T> result;
            int ix = -1;
            int iz = -1;
            int iy = m_ChildNodes.Length == 4 ? 0 : -1;
            int nodeIndex = 0;
            for (int i = ix; i <= 1; i += 2)
            {
                for (int k = iy; k <= 1; k += 2)
                {
                    for (int j = iz; j <= 1; j += 2)
                    {
                        result = CreateNode(ref m_ChildNodes[nodeIndex], depth, m_Bounds.center + new Vector3(i * m_HalfSize.x * 0.5f, k * m_HalfSize.y * 0.5f, j * m_HalfSize.z * 0.5f), m_HalfSize, obj);
                        if (result != null)
                        {
                            return result;
                        }
                        nodeIndex += 1;
                    }
                }
            }
            return null;
        }
        protected SceneTreeNode<T> CreateNode(ref SceneTreeNode<T> node, int depth, Vector3 centerPos, Vector3 size, T obj)
        {
            SceneTreeNode<T> result = null;
            if (node == null)
            {
                Bounds bounds = new Bounds(centerPos, size);
                if (bounds.IsBoundsContainsAnotherBounds(obj.Bounds))
                {
                    SceneTreeNode<T> newNode = new SceneTreeNode<T>(bounds, depth + 1, m_ChildNodes.Length);
                    node = newNode;
                    result = node;
                }
            } else if (node.Bounds.IsBoundsContainsAnotherBounds(obj.Bounds))
            {
                result = node;
            }
            return result;
        }
#if UNITY_EDITOR
        public void DrawNode(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj, int maxDepth)
        {
            if (m_ChildNodes != null)
            {
                foreach (var node in m_ChildNodes)
                {
                    node?.DrawNode(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, maxDepth);
                }
            }
            if (m_CurrentDepth >= drawMinDepth && m_CurrentDepth <= drawMaxDepth)
            {
                float d = ((float)m_CurrentDepth) / maxDepth;
                Color color = Color.Lerp(treeMinDepthColor, treeMaxDepthColor, d);
                m_Bounds.DrawBounds(color);
            }
            if (!drawObj) return;
            {
                var node = m_ObjectList.First;
                while (node != null)
                {
                    var sceneObj = node.Value as Scenable;
                    sceneObj?.DrawArea(objColor, hitObjColor);
                    node = node.Next;
                }
            }
        }
#endif
    }
}