using System;
using System.Collections.Generic;
using UnityEngine;
namespace GamePlay.Scene
{
    [Serializable]
    public class OctreeNode
    {
        [SerializeField]
        public Bounds nodeBounds; // 节点的包围盒
        
        [SerializeField]
        public float minSize; // 最小节点大小
        
        [SerializeField]
        public Bounds[] childBounds; // 子节点的包围盒数组
       
        [SerializeField]
        public OctreeNode[] children; // 子节点数组
        
        [SerializeField]
        public OctreeNode parent; // 父节点

        [SerializeField]
        public bool isLeaf; // 是否叶节点
        
        [SerializeField]
        public bool isRoot; // 是否根节点
        
        [SerializeField]
        public List<IOctrable> octrables; // 子节点数组

        public List<IOctrable> Octrables
        {
            get
            {
                return octrables ??= new List<IOctrable>();
            }
        }
        
        // 构造函数,接受一个包围盒和最小节点大小作为参数
        public OctreeNode(Bounds b, float minNodeSize,OctreeNode parent = null)
        {
            isLeaf = false;
            isRoot = false;
            nodeBounds = b;
            minSize = minNodeSize;
            float quarter = nodeBounds.size.y / 4.0f;
            float childLength = nodeBounds.size.y / 2;
            // 计算子节点的包围盒
            Vector3 childSize = new Vector3(childLength, childLength, childLength);
            childBounds = new Bounds[8];
            // 创建子节点的包围盒
            childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childSize);
            childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childSize);
            childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childSize);
            childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childSize);
            childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childSize);
            childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childSize);
            childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childSize);
            childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childSize);
            this.parent = parent;
        }


        // 将游戏对象添加到节点
        public void AddObject(IOctrable io)
        {
            DivideAndAdd(io);
        }

        // 分割并添加游戏对象
        public void DivideAndAdd(IOctrable io)
        {
            if (nodeBounds.size.y <= minSize)
            {
                // 如果节点大小小于等于最小节点大小,停止分割
                isLeaf = true;
                Octrables.Add(io);
                return; 
            }
            children ??= new OctreeNode[8];
            bool dividing = false;
            for (int i = 0; i < 8; i++)
            {
                children[i] ??= new OctreeNode(childBounds[i], minSize);

                // 如果游戏对象的包围盒与子节点的包围盒相交,进行分割
                if (!childBounds[i].Intersects(io.Collider.bounds)) continue;
                dividing = true;
                children[i].DivideAndAdd(io);
            }

            // 如果没有进行分割,将子节点数组设为null
            if (dividing == false)
            {
                children = null;
            }
        }

        // 绘制节点的包围盒
        public void Draw()
        {
            Gizmos.color = isLeaf ? new Color(1, 0, 0) : new Color(0, 1, 0);
            // LogManager.Log("OctreeNode",$"isLeaf:{isLeaf}");
            Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);

            // 如果子节点不为空,递归绘制子节点
            if (children == null) return;
            for (int i = 0; i < 8; i++)
            {
                if (children[i] != null)
                {
                    // 递归调用
                    children[i].Draw(); 
                }
            }
        }
    }
}