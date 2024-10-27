// author:KIPKIPS
// date:2024.10.26 18:23
// describe:
using UnityEngine;
using System.Collections.Generic;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 场景物体接口：需要插入到场景四叉树并实现动态显示与隐藏的物体实现该接口
    /// </summary>
    public interface IScenable
    {
        /// <summary>
        /// 该物体的包围盒
        /// </summary>
        Bounds Bounds { get; }
        /// <summary>
        /// 该物体进入显示区域时调用（在这里处理物体的加载或显示）
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        bool OnShow(Transform parent);
        /// <summary>
        /// 该物体离开显示区域时调用（在这里处理物体的卸载或隐藏）
        /// </summary>
        void OnHide();
    }
    public interface IScenableLinkedListNode
    {
        Dictionary<uint, object> GetNodes();
        LinkedListNode<T> GetLinkedListNode<T>(uint morton) where T : IScenable;
        void SetLinkedListNode<T>(uint morton, LinkedListNode<T> node);

        //void ClearLinkedListNode(int morton);
    }
}