// author:KIPKIPS
// date:2024.10.26 18:16
// describe:
using UnityEngine;
using Framework.Common;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 线性八叉树
    /// 节点字典存放叶节点Morton作为Key
    /// </summary>
    public class LinearOctree<T> : LinearTree<T> where T : IScenable, IScenableLinkedListNode
    {
        private readonly float m_DeltaX;
        private readonly float m_DeltaY;
        private readonly float m_DeltaZ;
        public LinearOctree(Vector3 center, Vector3 size, int maxDepth) : base(center, size, maxDepth)
        {
            m_DeltaX = m_Bounds.size.x / m_Cols;
            m_DeltaY = m_Bounds.size.y / m_Cols;
            m_DeltaZ = m_Bounds.size.z / m_Cols;
        }
        public override void Add(T item)
        {
            if (item == null)
            {
                return;
            }
            if (!m_Bounds.Intersects(item.Bounds)) return;
            if (m_MaxDepth == 0)
            {
                if (m_Nodes.ContainsKey(0) == false)
                {
                    m_Nodes[0] = new LinearTreeLeaf<T>();
                }
                var node = m_Nodes[0].Insert(item);
                item.SetLinkedListNode(0, node);
            } 
            else
            {
                InsertToNode(item, 0, m_Bounds.center.x, m_Bounds.center.y, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.y, m_Bounds.size.z);
            }
        }
        public override void Trigger(IDetector detector, TriggerHandle<T> handle)
        {
            if (handle == null)
            {
                return;
            }
            if (detector.UseCameraCulling)
            {
                //如果使用相机裁剪，则计算出八个角点的裁剪掩码，且子节点的裁剪检测可以复用部分父节点的角点
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
                TriggerToNodeByCamera(detector, handle, 0, code, m_Bounds.center.x, m_Bounds.center.y, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.y, m_Bounds.size.z);
            } 
            else
            {
                if (m_MaxDepth == 0)
                {
                    if (m_Nodes.ContainsKey(0) && m_Nodes[0] != null)
                    {
                        m_Nodes[0].Trigger(detector, handle);
                    }
                }
                else
                {
                    TriggerToNode(detector, handle, 0, m_Bounds.center.x, m_Bounds.center.y, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.y, m_Bounds.size.z);
                }
            }
        }
        private bool InsertToNode(T obj, int depth, float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton3FromWorldPos(centerX, centerY, centerZ);
                if (m_Nodes.ContainsKey(m) == false)
                {
                    m_Nodes[m] = new LinearTreeLeaf<T>();
                }
                var node = m_Nodes[m].Insert(obj);
                obj.SetLinkedListNode(m, node);
                return true;
            }
            int collider = 0;
            float minX = obj.Bounds.min.x;
            float minY = obj.Bounds.min.y;
            float minZ = obj.Bounds.min.z;
            float maxX = obj.Bounds.max.x;
            float maxY = obj.Bounds.max.y;
            float maxZ = obj.Bounds.max.z;
            if (minX <= centerX && minY <= centerY && minZ <= centerZ)
            {
                collider |= 1;
            }
            if (minX <= centerX && minY <= centerY && maxZ >= centerZ)
            {
                collider |= 2;
            }
            if (minX <= centerX && maxY >= centerY && minZ <= centerZ)
            {
                collider |= 4;
            }
            if (minX <= centerX && maxY >= centerY && maxZ >= centerZ)
            {
                collider |= 8;
            }
            if (maxX >= centerX && minY <= centerY && minZ <= centerZ)
            {
                collider |= 16;
            }
            if (maxX >= centerX && minY <= centerY && maxZ >= centerZ)
            {
                collider |= 32;
            }
            if (maxX >= centerX && maxY >= centerY && minZ <= centerZ)
            {
                collider |= 64;
            }
            if (maxX >= centerX && maxY >= centerY && maxZ >= centerZ)
            {
                collider |= 128;
            }
            float sx = sizeX * 0.5f, sy = sizeY * 0.5f, sz = sizeZ * 0.5f;
            bool insertResult = false;
            if ((collider & 1) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 2) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 4) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 8) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 16) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 32) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 64) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
            }
            if ((collider & 128) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
            return insertResult;
        }
        private void TriggerToNodeByCamera(IDetector detector, TriggerHandle<T> handle, int depth, TreeCullingCode cullingCode, float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ)
        {
            if (cullingCode.IsCulled())
                return;
            if (depth == m_MaxDepth)
            {
                uint m = Morton3FromWorldPos(centerX, centerY, centerZ);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            } else
            {
                float sx = sizeX * 0.5f, sy = sizeY * 0.5f, sz = sizeZ * 0.5f;
                int leftBottomMiddle = detector.GetDetectedCode(centerX - sx, centerY - sy, centerZ, true);
                int middleBottomMiddle = detector.GetDetectedCode(centerX, centerY - sy, centerZ, true);
                int rightBottomMiddle = detector.GetDetectedCode(centerX + sx, centerY - sy, centerZ, true);
                int middleBottomBack = detector.GetDetectedCode(centerX, centerY - sy, centerZ - sz, true);
                int middleBottomForward = detector.GetDetectedCode(centerX, centerY - sy, centerZ + sz, true);
                int leftMiddleBack = detector.GetDetectedCode(centerX - sx, centerY, centerZ - sz, true);
                int leftMiddleMiddle = detector.GetDetectedCode(centerX - sx, centerY, centerZ, true);
                int leftMiddleForward = detector.GetDetectedCode(centerX - sx, centerY, centerZ + sz, true);
                int middleMiddleBack = detector.GetDetectedCode(centerX, centerY, centerZ - sz, true);
                int middleMiddleMiddle = detector.GetDetectedCode(centerX, centerY, centerZ, true);
                int middleMiddleForward = detector.GetDetectedCode(centerX, centerY, centerZ + sz, true);
                int rightMiddleBack = detector.GetDetectedCode(centerX + sx, centerY, centerZ - sz, true);
                int rightMiddleMiddle = detector.GetDetectedCode(centerX + sx, centerY, centerZ, true);
                int rightMiddleForward = detector.GetDetectedCode(centerX + sx, centerY, centerZ + sz, true);
                int leftTopMiddle = detector.GetDetectedCode(centerX - sx, centerY + sy, centerZ, true);
                int middleTopMiddle = detector.GetDetectedCode(centerX, centerY + sy, centerZ, true);
                int rightTopMiddle = detector.GetDetectedCode(centerX + sx, centerY + sy, centerZ, true);
                int middleTopBack = detector.GetDetectedCode(centerX, centerY + sy, centerZ - sz, true);
                int middleTopForward = detector.GetDetectedCode(centerX, centerY + sy, centerZ + sz, true);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = cullingCode.leftBottomBack,
                    leftBottomForward = leftBottomMiddle,
                    leftTopBack = leftMiddleBack,
                    leftTopForward = leftMiddleMiddle,
                    rightBottomBack = middleBottomBack,
                    rightBottomForward = middleBottomMiddle,
                    rightTopBack = middleMiddleBack,
                    rightTopForward = middleMiddleMiddle,
                }, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = leftBottomMiddle,
                    leftBottomForward = cullingCode.leftBottomForward,
                    leftTopBack = leftMiddleMiddle,
                    leftTopForward = leftMiddleForward,
                    rightBottomBack = middleBottomMiddle,
                    rightBottomForward = middleBottomForward,
                    rightTopBack = middleMiddleMiddle,
                    rightTopForward = middleMiddleForward,
                }, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = leftMiddleBack,
                    leftBottomForward = leftMiddleMiddle,
                    leftTopBack = cullingCode.leftTopBack,
                    leftTopForward = leftTopMiddle,
                    rightBottomBack = middleMiddleBack,
                    rightBottomForward = middleMiddleMiddle,
                    rightTopBack = middleTopBack,
                    rightTopForward = middleTopMiddle,
                }, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = leftMiddleMiddle,
                    leftBottomForward = leftMiddleForward,
                    leftTopBack = leftTopMiddle,
                    leftTopForward = cullingCode.leftTopForward,
                    rightBottomBack = middleMiddleMiddle,
                    rightBottomForward = middleMiddleForward,
                    rightTopBack = middleTopMiddle,
                    rightTopForward = middleTopForward,
                }, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = middleBottomBack,
                    leftBottomForward = middleBottomMiddle,
                    leftTopBack = middleMiddleBack,
                    leftTopForward = middleMiddleMiddle,
                    rightBottomBack = cullingCode.rightBottomBack,
                    rightBottomForward = rightBottomMiddle,
                    rightTopBack = rightMiddleBack,
                    rightTopForward = rightMiddleMiddle,
                }, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = middleBottomMiddle,
                    leftBottomForward = middleBottomForward,
                    leftTopBack = middleMiddleMiddle,
                    leftTopForward = middleMiddleForward,
                    rightBottomBack = rightBottomMiddle,
                    rightBottomForward = cullingCode.rightBottomForward,
                    rightTopBack = rightMiddleMiddle,
                    rightTopForward = rightMiddleForward,
                }, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = middleMiddleBack,
                    leftBottomForward = middleMiddleMiddle,
                    leftTopBack = middleTopBack,
                    leftTopForward = middleTopMiddle,
                    rightBottomBack = rightMiddleBack,
                    rightBottomForward = rightMiddleMiddle,
                    rightTopBack = cullingCode.rightTopBack,
                    rightTopForward = rightTopMiddle,
                }, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                {
                    leftBottomBack = middleMiddleMiddle,
                    leftBottomForward = middleMiddleForward,
                    leftTopBack = middleTopMiddle,
                    leftTopForward = middleTopForward,
                    rightBottomBack = rightMiddleMiddle,
                    rightBottomForward = rightMiddleForward,
                    rightTopBack = rightTopMiddle,
                    rightTopForward = cullingCode.rightTopForward,
                }, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
        }
        private void TriggerToNode(IDetector detector, TriggerHandle<T> handle, int depth, float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton3FromWorldPos(centerX, centerY, centerZ);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            } else
            {
                int collider = detector.GetDetectedCode(centerX, centerY, centerZ, false);
                float sx = sizeX * 0.5f, sy = sizeY * 0.5f, sz = sizeZ * 0.5f;
                if ((collider & 1) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                if ((collider & 2) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                if ((collider & 4) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                if ((collider & 8) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                if ((collider & 16) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                if ((collider & 32) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerY - sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
                if ((collider & 64) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ - sz * 0.5f, sx, sy, sz);
                if ((collider & 128) != 0)
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerY + sy * 0.5f, centerZ + sz * 0.5f, sx, sy, sz);
            }
        }
        private uint Morton3FromWorldPos(float x, float y, float z)
        {
            uint px = (uint)Mathf.FloorToInt((x - m_Bounds.min.x) / m_DeltaX);
            uint py = (uint)Mathf.FloorToInt((y - m_Bounds.min.y) / m_DeltaY);
            uint pz = (uint)Mathf.FloorToInt((z - m_Bounds.min.z) / m_DeltaZ);
            return Morton3(px, py, pz);
        }
        private uint Morton3(uint x, uint y, uint z)
        {
            return (Part1By2(z) << 2) + (Part1By2(y) << 1) + Part1By2(x);
        }
        private uint Part1By2(uint n)
        {
            n = (n ^ (n << 16)) & 0xff0000ff;
            n = (n ^ (n << 8)) & 0x0300f00f;
            n = (n ^ (n << 4)) & 0x030c30c3;
            n = (n ^ (n << 2)) & 0x09249249;
            return n;
        }
        public static implicit operator bool(LinearOctree<T> tree)
        {
            return tree != null;
        }
#if UNITY_EDITOR
        public override void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj)
        {
            DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, 0, m_Bounds.center, m_Bounds.size);
        }
        private bool DrawNodeGizmos(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj, int depth, Vector3 center, Vector3 size)
        {
            if (depth < drawMinDepth || depth > drawMaxDepth)
                return false;
            float d = ((float)depth) / m_MaxDepth;
            Color color = Color.Lerp(treeMinDepthColor, treeMaxDepthColor, d);
            if (depth == m_MaxDepth)
            {
                uint m = Morton3FromWorldPos(center.x, center.y, center.z);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    if (m_Nodes[m].DrawNode(objColor, hitObjColor, drawObj))
                    {
                        Bounds b = new Bounds(new Vector3(center.x, center.y, center.z), new Vector3(size.x, size.y, size.z));
                        b.DrawBounds(color);
                        return true;
                    }
                }
            } else
            {
                bool draw = false;
                float sx = size.x * 0.5f, sy = size.y * 0.5f, sz = size.z * 0.5f;
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x - sx * 0.5f, center.y - sy * 0.5f, center.z - sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x - sx * 0.5f, center.y - sy * 0.5f, center.z + sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x - sx * 0.5f, center.y + sy * 0.5f, center.z - sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x - sx * 0.5f, center.y + sy * 0.5f, center.z + sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x + sx * 0.5f, center.y - sy * 0.5f, center.z - sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x + sx * 0.5f, center.y - sy * 0.5f, center.z + sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x + sx * 0.5f, center.y + sy * 0.5f, center.z - sz * 0.5f), new Vector3(sx, sy, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector3(center.x + sx * 0.5f, center.y + sy * 0.5f, center.z + sz * 0.5f), new Vector3(sx, sy, sz));
                if (!draw) return false;
                Bounds b = new Bounds(new Vector3(center.x, center.y, center.z), new Vector3(size.x, size.y, size.z));
                b.DrawBounds(color);
                return true;
            }
            return false;
        }
#endif
    }
}