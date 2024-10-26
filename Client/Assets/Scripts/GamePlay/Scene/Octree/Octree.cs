using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Scene
{
    [Serializable]
    public class Octree
    {
        [SerializeField]
        public OctreeNode rootNode; // 八叉树的根节点 
        private const string LOGTag = "Octree";
        // 构造函数,接受世界中的游戏对象数组和最小节点大小作为参数
        public Octree(IOctrable[] worldObjects, float minNodeSize)
        {
            GeneralOctree(worldObjects,minNodeSize);
            // 将世界中的游戏对象添加到八叉树中
            AddObjects(worldObjects);
        }

        public void GeneralOctree(IOctrable[] worldObjects, float minNodeSize)
        {
            LogManager.Log(LOGTag,$"worldObjects count:{worldObjects.Length}");
            Bounds bounds = new Bounds(); // 用于计算包围盒的 Bounds 对象

            // 遍历所有游戏对象,计算包围盒以包含它们
            foreach (IOctrable octreeItem in worldObjects)
            {
                LogManager.Log(LOGTag,$"Item name:{octreeItem.SelfTrs.name}");
                bounds.Encapsulate(octreeItem.Collider.bounds);
            }

            // 计算包围盒的最大边长
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 0.5f;

            // 将包围盒的最小和最大点调整为形成一个正方体
            bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);

            // 创建八叉树的根节点,传入包围盒和最小节点大小
            rootNode = new OctreeNode(bounds, minNodeSize);
            rootNode.isRoot = true;
        }

        // 将游戏对象添加到八叉树中
        public void AddObjects(IEnumerable<IOctrable> newObjects)
        {
            foreach (IOctrable o in newObjects)
            {
                AddObject(o);
            }
        }
        public void AddObject(IOctrable newObject)
        {
            rootNode.AddObject(newObject);
        }

        public List<OctreeNode> CheckBounds(Bounds checkBounds)
        {
            List<OctreeNode> nodes = new List<OctreeNode>();
            CheckNode(rootNode, checkBounds,ref nodes);
            return nodes;
        }
        private void CheckNode(OctreeNode node, Bounds checkBounds,ref List<OctreeNode> nodes)
        {
            if (node == null)
            {
                return;
            }
            if (!node.nodeBounds.Intersects(checkBounds)) return;
            if (node.isLeaf)
            {
                nodes.Add(node);
            } 
            else
            {
                if (node.children == null) return;
                foreach (var child in node.children)
                {
                    CheckNode(child, checkBounds,ref nodes);
                }
            }
        }
    }
}