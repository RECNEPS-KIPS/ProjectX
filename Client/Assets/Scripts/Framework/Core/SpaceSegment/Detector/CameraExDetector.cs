// author:KIPKIPS
// date:2024.10.26 18:41
// describe:
using Framework.Common;
using UnityEngine;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 该触发器根据相机裁剪区域触发，且根据相机运动趋势改变裁剪区域
    /// </summary>
    public class CameraExDetector : CameraDetector
    {
        #region 裁剪区域扩展趋势参数

        /// <summary>
        /// 左侧扩展距离，当相机往左右移动时的裁剪区域扩展
        /// </summary>
        public float leftExtDis;
        /// <summary>
        /// 右侧扩展距离，当相机往左右移动时的裁剪区域扩展
        /// </summary>
        public float rightExtDis;
        /// <summary>
        /// 顶部方向扩展距离，当相机往前移动时的裁剪区域扩展
        /// </summary>
        public float topExtDis;
        /// <summary>
        /// 底部方向扩展距离，当相机往后移动时的裁剪区域扩展
        /// </summary>
        public float bottomExtDis;

        #endregion

        private Vector3 m_Position;
        private float m_LeftEx;
        private float m_RightEx;
        private float m_UpEx;
        private float m_DownEx;
        void Start()
        {
            m_Camera = gameObject.GetComponent<Camera>();
            m_Position = transform.position;
            //m_Codes = new int[27];
        }
        void Update()
        {
            Transform transform1;
            Vector3 moveDir = -(transform1 = transform).worldToLocalMatrix.MultiplyPoint(m_Position);
            m_Position = transform1.position;
            m_LeftEx = moveDir.x < -Mathf.Epsilon ? -leftExtDis : 0;
            m_RightEx = moveDir.x > Mathf.Epsilon ? rightExtDis : 0;
            m_UpEx = moveDir.y > Mathf.Epsilon ? topExtDis : 0;
            m_DownEx = moveDir.y < -Mathf.Epsilon ? -bottomExtDis : 0;
        }
        public override bool IsDetected(Bounds bounds)
        {
            if (m_Camera == null)
            {
                return false;
            }
            return bounds.IsBoundsInCameraEx(m_Camera, m_LeftEx, m_RightEx, m_DownEx, m_UpEx);
        }
        protected override int CalculateCullCode(Vector4 position, Matrix4x4 matrix)
        {
            return MathUtils.ComputeOutCodeEx(position, matrix, m_LeftEx, m_RightEx, m_DownEx, m_UpEx);
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Camera camera = gameObject.GetComponent<Camera>();
            if (camera)
            {
                GizmosUtils.DrawViewFrustumEx(camera, m_LeftEx, m_RightEx, m_DownEx, m_UpEx, Color.yellow);
            }
        }
#endif
    }
}