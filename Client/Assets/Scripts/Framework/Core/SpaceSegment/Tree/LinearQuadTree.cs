// author:KIPKIPS
// date:2024.10.26 18:18
// describe:
using Framework.Common;
using UnityEngine;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 线性四叉树
    /// 节点字典存放叶节点Morton作为Key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinearQuadTree<T> : LinearTree<T> where T : IScenable, IScenableLinkedListNode
    {
        private readonly float m_DeltaWidth;
        private readonly float m_DeltaHeight;
        public LinearQuadTree(Vector3 center, Vector3 size, int maxDepth) : base(center, size, maxDepth)
        {
            m_DeltaWidth = m_Bounds.size.x / m_Cols;
            m_DeltaHeight = m_Bounds.size.z / m_Cols;
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
                    m_Nodes[0] = new LinearTreeLeaf<T>();
                var node = m_Nodes[0].Insert(item);
                item.SetLinkedListNode(0, node);
            } else
            {
                InsertToNode(item, 0, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
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
                TriggerToNodeByCamera(detector, handle, 0, code, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
            } else
            {
                if (m_MaxDepth == 0)
                {
                    if (m_Nodes.ContainsKey(0) && m_Nodes[0] != null)
                    {
                        m_Nodes[0].Trigger(detector, handle);
                    }
                } else
                {
                    TriggerToNode(detector, handle, 0, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
                }
            }
        }
        private bool InsertToNode(T obj, int depth, float centerX, float centerZ, float sizeX, float sizeZ)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(centerX, centerZ);
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
            float minZ = obj.Bounds.min.z;
            float maxX = obj.Bounds.max.x;
            float maxZ = obj.Bounds.max.z;
            if (minX <= centerX && minZ <= centerZ)
            {
                collider |= 1;
            }
            if (minX <= centerX && maxZ >= centerZ)
            {
                collider |= 2;
            }
            if (maxX >= centerX && minZ <= centerZ)
            {
                collider |= 4;
            }
            if (maxX >= centerX && maxZ >= centerZ)
            {
                collider |= 8;
            }
            float sx = sizeX * 0.5f, sz = sizeZ * 0.5f;
            bool insertResult = false;
            if ((collider & 1) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
            }
            if ((collider & 2) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX - sx * 0.5f, centerZ + sz * 0.5f, sx, sz);
            }
            if ((collider & 4) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
            }
            if ((collider & 8) != 0)
            {
                insertResult |= InsertToNode(obj, depth + 1, centerX + sx * 0.5f, centerZ + sz * 0.5f, sx, sz);
            }
            return insertResult;
        }
        private void TriggerToNodeByCamera(IDetector detector, TriggerHandle<T> handle, int depth, TreeCullingCode cullingCode, float centerX, float centerZ, float sizeX, float sizeZ)
        {
            while (true)
            {
                if (cullingCode.IsCulled()) return;
                if (depth == m_MaxDepth)
                {
                    uint m = Morton2FromWorldPos(centerX, centerZ);
                    if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                    {
                        m_Nodes[m].Trigger(detector, handle);
                    }
                } else
                {
                    float sx = sizeX * 0.5f, sz = sizeZ * 0.5f;
                    int leftBottomMiddle = detector.GetDetectedCode(centerX - sx, m_Bounds.min.y, centerZ, true);
                    int middleBottomMiddle = detector.GetDetectedCode(centerX, m_Bounds.min.y, centerZ, true);
                    int rightBottomMiddle = detector.GetDetectedCode(centerX + sx, m_Bounds.min.y, centerZ, true);
                    int middleBottomBack = detector.GetDetectedCode(centerX, m_Bounds.min.y, centerZ - sz, true);
                    int middleBottomForward = detector.GetDetectedCode(centerX, m_Bounds.min.y, centerZ + sz, true);
                    int leftTopMiddle = detector.GetDetectedCode(centerX - sx, m_Bounds.max.y, centerZ, true);
                    int middleTopMiddle = detector.GetDetectedCode(centerX, m_Bounds.max.y, centerZ, true);
                    int rightTopMiddle = detector.GetDetectedCode(centerX + sx, m_Bounds.max.y, centerZ, true);
                    int middleTopBack = detector.GetDetectedCode(centerX, m_Bounds.max.y, centerZ - sz, true);
                    int middleTopForward = detector.GetDetectedCode(centerX, m_Bounds.max.y, centerZ + sz, true);
                    TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                    {
                        leftBottomBack = cullingCode.leftBottomBack,
                        leftBottomForward = leftBottomMiddle,
                        leftTopBack = cullingCode.leftTopBack,
                        leftTopForward = leftTopMiddle,
                        rightBottomBack = middleBottomBack,
                        rightBottomForward = middleBottomMiddle,
                        rightTopBack = middleTopBack,
                        rightTopForward = middleTopMiddle,
                    }, centerX - sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
                    TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                    {
                        leftBottomBack = leftBottomMiddle, leftBottomForward = cullingCode.leftBottomForward, leftTopBack = leftTopMiddle, leftTopForward = cullingCode.leftTopForward,
                        rightBottomBack = middleBottomMiddle, rightBottomForward = middleBottomForward, rightTopBack = middleTopMiddle, rightTopForward = middleTopForward,
                    }, centerX - sx * 0.5f, centerZ + sz * 0.5f, sx, sz);
                    TriggerToNodeByCamera(detector, handle, depth + 1, new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomBack, leftBottomForward = middleBottomMiddle, leftTopBack = middleTopBack, leftTopForward = middleTopMiddle,
                        rightBottomBack = cullingCode.rightBottomBack, rightBottomForward = rightBottomMiddle, rightTopBack = cullingCode.rightTopBack, rightTopForward = rightTopMiddle,
                    }, centerX + sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
                    depth = depth + 1;
                    cullingCode = new TreeCullingCode()
                    {
                        leftBottomBack = middleBottomMiddle, leftBottomForward = middleBottomForward, leftTopBack = middleTopMiddle, leftTopForward = middleTopForward,
                        rightBottomBack = rightBottomMiddle, rightBottomForward = cullingCode.rightBottomForward, rightTopBack = rightTopMiddle, rightTopForward = cullingCode.rightTopForward,
                    };
                    centerX = centerX + sx * 0.5f;
                    centerZ = centerZ + sz * 0.5f;
                    sizeX = sx;
                    sizeZ = sz;
                    continue;
                }
                break;
            }
        }
        private void TriggerToNode(IDetector detector, TriggerHandle<T> handle, int depth, float centerX, float centerZ, float sizeX, float sizeZ)
        {
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(centerX, centerZ);
                if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
                {
                    m_Nodes[m].Trigger(detector, handle);
                }
            } 
            else
            {
                int collider = detector.GetDetectedCode(centerX, m_Bounds.center.y, centerZ, true);
                float sx = sizeX * 0.5f, sz = sizeZ * 0.5f;
                if ((collider & 1) != 0)
                {
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
                }
                if ((collider & 2) != 0)
                {
                    TriggerToNode(detector, handle, depth + 1, centerX - sx * 0.5f, centerZ + sz * 0.5f, sx, sz);
                }
                if ((collider & 4) != 0)
                {
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerZ - sz * 0.5f, sx, sz);
                }
                if ((collider & 8) != 0)
                {
                    TriggerToNode(detector, handle, depth + 1, centerX + sx * 0.5f, centerZ + sz * 0.5f, sx, sz);
                }
            }
        }
        private uint Morton2FromWorldPos(float x, float z)
        {
            uint px = (uint)Mathf.FloorToInt((x - m_Bounds.min.x) / m_DeltaWidth);
            uint pz = (uint)Mathf.FloorToInt((z - m_Bounds.min.z) / m_DeltaHeight);
            return Morton2(px, pz);
        }
        private uint Morton2(uint x, uint y)
        {
            return (Part1By1(y) << 1) + Part1By1(x);
        }
        private uint Part1By1(uint n)
        {
            n = (n ^ (n << 8)) & 0x00ff00ff;
            n = (n ^ (n << 4)) & 0x0f0f0f0f;
            n = (n ^ (n << 2)) & 0x33333333;
            n = (n ^ (n << 1)) & 0x55555555;
            return n;
        }
        public static implicit operator bool(LinearQuadTree<T> tree)
        {
            return tree != null;
        }
#if UNITY_EDITOR
        public override void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj)
        {
            DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, 0, new Vector2(m_Bounds.center.x, m_Bounds.center.z), new Vector2(m_Bounds.size.x, m_Bounds.size.z));
        }
        private bool DrawNodeGizmos(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj, int depth, Vector2 center, Vector2 size)
        {
            if (depth < drawMinDepth || depth > drawMaxDepth)
            {
                return false;
            }
            float d = (float)depth / m_MaxDepth;
            Color color = Color.Lerp(treeMinDepthColor, treeMaxDepthColor, d);
            if (depth == m_MaxDepth)
            {
                uint m = Morton2FromWorldPos(center.x, center.y);
                if (!m_Nodes.ContainsKey(m) || m_Nodes[m] == null) return false;
                if (!m_Nodes[m].DrawNode(objColor, hitObjColor, drawObj)) return false;
                Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y), new Vector3(size.x, m_Bounds.size.y, size.y));
                b.DrawBounds(color);
                return true;
            } 
            else
            {
                bool draw = false;
                float sx = size.x * 0.5f, sz = size.y * 0.5f;
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));
                draw |= DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));
                if (!draw) return false;
                Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y), new Vector3(size.x, m_Bounds.size.y, size.y));
                b.DrawBounds(color);
                return true;
            }
        }
#endif
    }
}