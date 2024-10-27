// author:KIPKIPS
// date:2024.10.26 18:46
// describe:
using Framework.Common;
using UnityEngine;

namespace Framework.Core.SpaceSegment
{
    /// <summary>
    /// 该触发器根据Transform的包围盒区域触发-且根据Transform运动趋势扩展包围盒
    /// </summary>
    public class TransformExDetector : TransformDetector
    {
        #region 包围盒扩展趋势参数

        public float leftExtDis;
        public float rightExtDis;
        public float topExtDis;
        public float bottomExtDis;

        #endregion

        private Vector3 m_Position;
        private Vector3 m_PosOffset;
        private Vector3 m_SizeEx;
        void Start()
        {
            m_Position = transform.position;
        }
        void Update()
        {
            var position = transform.position;
            Vector3 moveDir = position - m_Position;
            m_Position = position;
            float xex = 0, zex = 0;
            if (moveDir.x < -Mathf.Epsilon)
                xex = -leftExtDis;
            else if (moveDir.x > Mathf.Epsilon)
                xex = rightExtDis;
            else
                xex = 0;
            if (moveDir.z < -Mathf.Epsilon)
                zex = -bottomExtDis;
            else if (moveDir.z > Mathf.Epsilon)
                zex = topExtDis;
            else
                zex = 0;
            m_PosOffset = new Vector3(xex * 0.5f, 0, zex * 0.5f);
            m_SizeEx = new Vector3(Mathf.Abs(xex), 0, Mathf.Abs(zex));
        }
        protected override void RefreshBounds()
        {
            m_Bounds.center = Position + m_PosOffset;
            m_Bounds.size = detectorSize + m_SizeEx;
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Bounds b = new Bounds(transform.position + m_PosOffset, detectorSize + m_SizeEx);
            b.DrawBounds(Color.yellow);
        }
#endif
    }
}