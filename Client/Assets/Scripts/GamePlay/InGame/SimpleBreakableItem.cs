// author:KIPKIPS
// date:2024.10.17 11:32
// describe:用Voronoi库生成碎裂的Mesh

using UnityEngine;
using System.Collections.Generic;
using Plugins.Voronoi;

namespace GamePlay.InGame
{
    /// <summary>
    /// 使用delaunay三角剖分将墙壁/表面分割成独立的碎片
    /// </summary>
    public class SimpleBreakableItem : MonoBehaviour
    {
        [Tooltip("分割的数量")] public int Detail = 40;
        [Tooltip("只创建表面的mesh,远处的物体可以勾上,节省性能")] public bool OnlySurface;
        [Tooltip("生成切分Mesh的Layer")] public LayerMask ShardLayer;

        private void Start()
        {
            var regions = CreateRegions(out var offset);
            Subdivide(regions, transform.localScale, offset);
            //原物体隐藏掉
            GetComponent<MeshRenderer>().enabled = false;
        }

        /// <summary>
        /// 创建要用作细分的区域 库链接(https://github.com/PouletFrit/csDelaunay)
        /// </summary>
        /// <returns>生成区域的列表</returns>
        private List<List<Vector2f>> CreateRegions(out Vector2 Offset)
        {
            //用父节点缩放
            var scale = transform.localScale;
            var left = -scale.x / 2.0f;
            var right = scale.x / 2.0f;
            var down = -scale.y / 2.0f;
            var up = scale.y / 2.0f;

            Offset = new Vector2(left, down);
            var bounds = new Rectf(0, 0, right - left, up - down);
            var sites = new List<Vector2f>();

            //创建Detail个随机的点作为三角形的中心
            for (var i = 0; i < Detail; i++)
            {
                var x = Random.Range(0, right - left);
                var y = Random.Range(0, up - down);
                var point = new Vector2f(x, y);
                sites.Add(point);
            }

            //将生成的数据传给库函数进行处理
            return new Voronoi(sites, bounds, 1).Regions();
        }

        /// <summary>
        /// 拆分网格
        /// </summary>
        /// <param name="Regions">delaunay三角剖分生成的区域数组</param>
        /// <param name="Scale">切分子物体的scale,通常是父节点的scale</param>
        /// <param name="Offset">偏移值</param>
        private void Subdivide(List<List<Vector2f>> Regions, Vector3 Scale, Vector2 Offset)
        {
            foreach (var region in Regions)
            {
                var len = region.Count;
                var trisCount = 12 * (len - 1);
                var tris = new int[trisCount];
                var verts = new Vector3[3 * len];
                var uvs = new Vector2[3 * len];

                AppendVerticesAndUVs(verts, uvs, region, Scale, Offset);
                AppendTriangles(tris, len);
                CreateShard(tris, verts, uvs);
            }
        }

        /// <summary>
        /// 生成新网格的顶点和uv
        /// </summary>
        /// <param name="Vertices">Mesh顶点</param>
        /// <param name="UVs">Mesh UVs</param>
        /// <param name="Region">delaunay三角切分算法生成的区域</param>
        /// <param name="Scale">切分网格的scale</param>
        /// <param name="Offset">偏移量</param>
        private void AppendVerticesAndUVs(Vector3[] Vertices, Vector2[] UVs, List<Vector2f> Region, Vector3 Scale,
            Vector2 Offset)
        {
            for (var i = 0; i < Region.Count; ++i)
            {
                var coord = Region[i];
                int one = i, two = Region.Count + i, three = 2 * Region.Count + i;
                Vertices[one] = new Vector3(coord.x + Offset.x, coord.y + Offset.y, Scale.z / 2.0f);
                Vertices[two] = new Vector3(coord.x + Offset.x, coord.y + Offset.y, -Scale.z / 2.0f);
                Vertices[three] = new Vector3(coord.x + Offset.x, coord.y + Offset.y, -Scale.z / 2.0f);

                var scaleX = (float)1f / Scale.x;
                var scaleY = (float)1f / Scale.y;
                UVs[one] = new Vector2(scaleX, scaleY);
                UVs[two] = new Vector2(coord.x * -scaleX, coord.y * -scaleY);
                UVs[three] = new Vector2(scaleX, scaleY);
            }
        }

        /// <summary>
        /// 生成切分网格的三角形
        /// </summary>
        /// <param name="Tris">网格或三角形的索引</param>
        /// <param name="Len">区域数组的长度</param>
        private void AppendTriangles(int[] Tris, int Len)
        {
            var t = 0;
            //添加正面多边形的碎片
            for (var v = 1; v < Len - 1; v++)
            {
                Tris[t++] = Len + v + 1;
                Tris[t++] = Len + v;
                Tris[t++] = Len;

                Tris[t++] = 2 * Len;
                Tris[t++] = 2 * Len + v;
                Tris[t++] = 2 * Len + v + 1;
            }

            //如果未启用OnlySurface表面优化 添加剩余的面,这里没有加背面
            if (OnlySurface) return;
            //创建其余的面
            for (var v = 0; v < Len; v++)
            {
                var n = v == (Len - 1) ? 0 : v + 1;

                Tris[t++] = v;
                Tris[t++] = Len + v;
                Tris[t++] = Len + n;

                Tris[t++] = v;
                Tris[t++] = Len + n;
                Tris[t++] = n;
            }
        }

        /// <summary>
        /// 创建并实例化墙的碎片对象
        /// </summary>
        /// <param name="Tris">Mesh</param>
        /// <param name="Vertices">Mesh顶点数组</param>
        /// <param name="UVs">Mesh UV数组</param>
        private void CreateShard(int[] Tris, Vector3[] Vertices, Vector2[] UVs)
        {
            var mesh = BuildMeshWithoutSharedVertices(Tris, Vertices, UVs);
            var shard = new GameObject();
            var t = transform;
            shard.name = t.name + "_Shard";
            shard.transform.SetParent(t, false);
            shard.transform.localPosition = Vector3.zero;
            shard.transform.localRotation = Quaternion.identity;
            shard.transform.localScale = Vector3.one;
            shard.layer = LayerMaskToLayer(ShardLayer);
            shard.AddComponent<MeshCollider>();
            shard.AddComponent<MeshFilter>();
            shard.AddComponent<MeshRenderer>();
            shard.AddComponent<Rigidbody>();
            shard.GetComponent<Rigidbody>().isKinematic = true;
            shard.GetComponent<MeshCollider>().convex = true;
            shard.GetComponent<MeshCollider>().sharedMesh = mesh;
            shard.GetComponent<MeshFilter>().sharedMesh = mesh;
            shard.GetComponent<Renderer>().material = t.GetComponent<Renderer>().material;
            t.localScale = Vector3.one;
        }

        private int LayerMaskToLayer(LayerMask Layer)
        {
            var n = Layer.value;
            var mask = 0;
            while (n > 1)
            {
                n = n >> 1;
                mask++;
            }

            return mask;
        }

        /// <summary>
        /// 创建一个没有共享顶点的网格 每个三角形都可以有独立的法线
        /// </summary>
        /// <param name="Tris">Mesh</param>
        /// <param name="Vertices">Mesh顶点数组</param>
        /// <param name="UVs">Mesh UV数组</param>
        /// <returns>Mesh对象</returns>
        private Mesh BuildMeshWithoutSharedVertices(int[] Tris, Vector3[] Vertices, Vector2[] UVs)
        {
            var newVertices = new List<Vector3>(Vertices);
            var newUVs = new List<Vector2>(UVs);
            var visited = new HashSet<int>();
            //遍历所有三角形,如果两个三角形共享一个顶点,只创建一个相同的顶点
            for (var i = 0; i < Tris.Length; ++i)
            {
                if (visited.Contains(Tris[i]))
                {
                    //复制顶点 不再被共享
                    newVertices.Add(Vertices[Tris[i]]);
                    newUVs.Add(UVs[Tris[i]]);
                    Tris[i] = newVertices.Count - 1;
                }

                visited.Add(Tris[i]);
            }

            //创建网格
            var mesh = new Mesh
            {
                vertices = newVertices.ToArray(),
                uv = newUVs.ToArray(),
                triangles = Tris
            };
            //计算法线。
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}