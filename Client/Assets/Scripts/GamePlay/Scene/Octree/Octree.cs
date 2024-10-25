namespace GamePlay.Scene
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
 
    public class Octree
    {
        public OctreeNode rootNode; // 八叉树的根节点 
 
        // 构造函数,接受世界中的游戏对象数组和最小节点大小作为参数
        public Octree(GameObject[] worldObjects, float minNodeSize)
        {
            Bounds bounds = new Bounds(); // 用于计算包围盒的 Bounds 对象
 
            // 遍历所有游戏对象,计算包围盒以包含它们
            foreach (GameObject go in worldObjects)
            {
                bounds.Encapsulate(go.GetComponent<Collider>().bounds);
            }
 
            // 计算包围盒的最大边长
            float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z });
            Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 0.5f;
 
            // 将包围盒的最小和最大点调整为形成一个正方体
            bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);
 
            // 创建八叉树的根节点,传入包围盒和最小节点大小
            rootNode = new OctreeNode(bounds, minNodeSize);
 
            // 将世界中的游戏对象添加到八叉树中
            AddObjects(worldObjects);
        }
 
        // 将游戏对象添加到八叉树中
        public void AddObjects(GameObject[] worldObjects)
        {
            foreach (GameObject go in worldObjects)
            {
                rootNode.AddObject(go);
            }
        }
    }

}