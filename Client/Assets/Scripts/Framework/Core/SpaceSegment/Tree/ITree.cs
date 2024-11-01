// author:KIPKIPS
// date:2024.10.26 18:04
// describe:
using UnityEngine;

namespace Framework.Core.SpaceSegment
{
    public delegate void TriggerHandle<T>(T trigger);
    /// <summary>
    /// 场景树接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITree<T> where T : IScenable, IScenableLinkedListNode
    {
        /// <summary>
        /// 树的根节点包围盒
        /// </summary>
        Bounds Bounds { get; }
        /// <summary>
        /// 树的最大深度
        /// </summary>
        int MaxDepth { get; }
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void Remove(T item);
        void Trigger(IDetector detector, TriggerHandle<T> handle);
#if UNITY_EDITOR
        void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj);
#endif
    }
    public struct TreeCullingCode
    {
        public int leftBottomBack;
        public int leftBottomForward;
        public int leftTopBack;
        public int leftTopForward;
        public int rightBottomBack;
        public int rightBottomForward;
        public int rightTopBack;
        public int rightTopForward;
        public bool IsCulled()
        {
            return (leftBottomBack & leftBottomForward & leftTopBack & leftTopForward & rightBottomBack & rightBottomForward & rightTopBack & rightTopForward) != 0;
        }
    }
}