using UnityEngine;

namespace GamePlay.Scene
{
    public class CreateOctree: MonoBehaviour
    {
        public GameObject[] worldObjects; // 存储世界中的游戏对象数组
        public int nodeMinsize = 5; // 八叉树的最小节点大小
        Octree otree; // 八叉树对象
 
        // 在启动时调用,用于初始化
        void Start()
        {
            otree = new Octree(worldObjects, nodeMinsize); // 创建八叉树对象并初始化
        }
 
        // 在每一帧更新时调用
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                otree.rootNode.Draw(); // 在运行时绘制八叉树的根节点的包围盒
            }
        }
    }
}