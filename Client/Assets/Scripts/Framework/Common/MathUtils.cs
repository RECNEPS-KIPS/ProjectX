// author:KIPKIPS
// describe:数学工具
using System;
using UnityEngine;

namespace Framework.Common
{
    /// <summary>
    /// 数学工具
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// 洗牌算法,传入目标列表,返回打乱顺序之后的列表,支持泛型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="seed"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ShuffleCoords<T>(T[] list, int seed = 0)
        {
            var random = new System.Random(seed); //根据随机种子获得一个随机数
            //遍历并随机交换
            for (var i = 0; i < list.Length - 1; i++)
            {
                var randomIndex = random.Next(i, list.Length); //返回一个随机的索引
                (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
            }
            return list;
        }
        public static int ComputeOutCode(Vector4 pos, Matrix4x4 projection)
        {
            pos = projection * pos;
            int code = 0;
            if (pos.x < -pos.w) code |= 0x01;
            if (pos.x > pos.w) code |= 0x02;
            if (pos.y < -pos.w) code |= 0x04;
            if (pos.y > pos.w) code |= 0x08;
            if (pos.z < -pos.w) code |= 0x10;
            if (pos.z > pos.w) code |= 0x20;
            return code;
        }
        public static int ComputeOutCodeEx(Vector4 pos, Matrix4x4 projection, float leftExt, float rightExt, float downExt, float upExt)
        {
            pos = projection * pos;
            int code = 0;
            if (pos.x < (-1 + leftExt) * pos.w) code |= 0x01;
            if (pos.x > (1 + rightExt) * pos.w) code |= 0x02;
            if (pos.y < (-1 + downExt) * pos.w) code |= 0x04;
            if (pos.y > (1 + upExt) * pos.w) code |= 0x08;
            if (pos.z < -pos.w) code |= 0x10;
            if (pos.z > pos.w) code |= 0x20;
            return code;
        }
        /// <summary>
        /// 绘制包围盒
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="color"></param>
        public static void DrawBounds(this Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        /// <summary>
        /// 判断包围盒是否被相机裁剪
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool IsBoundsInCamera(this Bounds bounds, Camera camera)
        {
            Matrix4x4 matrix = camera.projectionMatrix * camera.worldToCameraMatrix;
            int code = ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix);
            code &= ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix);
            return code == 0;
        }
        public static bool IsBoundsInCameraEx(this Bounds bounds, Camera camera, float leftExt, float rightExt, float downExt, float upExt)
        {
            Matrix4x4 matrix = camera.projectionMatrix * camera.worldToCameraMatrix;
            int code = ComputeOutCodeEx(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z + bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            code &= ComputeOutCodeEx(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2, bounds.center.z - bounds.size.z / 2, 1), matrix, leftExt, rightExt, downExt, upExt);
            return code == 0;
        }
        /// <summary>
        /// 判断包围盒是否包含另一个包围盒
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
        public static bool IsBoundsContainsAnotherBounds(this Bounds bounds, Bounds compareTo)
        {
            if (!bounds.Contains(compareTo.center + new Vector3(-compareTo.size.x / 2, compareTo.size.y / 2, -compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(compareTo.size.x / 2, compareTo.size.y / 2, -compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(compareTo.size.x / 2, compareTo.size.y / 2, compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(-compareTo.size.x / 2, compareTo.size.y / 2, compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(-compareTo.size.x / 2, -compareTo.size.y / 2, -compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(compareTo.size.x / 2, -compareTo.size.y / 2, -compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(compareTo.size.x / 2, -compareTo.size.y / 2, compareTo.size.z / 2)))
            {
                return false;
            }
            if (!bounds.Contains(compareTo.center + new Vector3(-compareTo.size.x / 2, -compareTo.size.y / 2, compareTo.size.z / 2)))
            {
                return false;
            }
            return true;
        }
        
        public static bool IsPowerOf2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        public static int ClosestPowerOf2(int value)
        {
            var next = (int)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));
            return next;
        }

        public static int NextPowerOf2(int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }
    }
}