using UnityEngine;

namespace Player
{
    public class PlayerMono : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D m_rigidbody2D;
        [SerializeField]
        private Collider2D m_dmgCollider;

        [Header("移动参数")]
        [SerializeField]
        private float m_moveSpeed = 5f;

        [Header("冲刺参数")]
        [SerializeField]
        private float m_dashSpeed = 15f;  // 冲刺速度
        [SerializeField]
        private float m_dashDuration = 0.3f;  // 冲刺持续时间
        [SerializeField]
        private float m_dashCooldown = 1f;  // 冲刺冷却时间

        private float m_horizontalInput;
        private float m_verticalInput;
        
        // 添加是否接收输入的标志
        private bool m_inputEnabled = true;

        // 冲刺相关变量
        private bool m_isDashing = false;
        private bool m_canDash = true;
        private Vector2 m_dashDirection;
        private float m_dashTimeLeft;
        private float m_dashCooldownTimeLeft;

        // 公共方法，设置是否启用输入
        public void SetInputEnabled(bool enabled)
        {
            m_inputEnabled = enabled;
            
            // 如果禁用输入，立即停止移动
            if (!m_inputEnabled && m_rigidbody2D != null)
            {
                m_rigidbody2D.velocity = Vector2.zero;
            }
        }

        // 公共属性，获取或设置输入启用状态
        public bool InputEnabled
        {
            get => m_inputEnabled;
            set => SetInputEnabled(value);
        }

        private void Update()
        {
            // 只在输入启用时获取输入
            if (m_inputEnabled)
            {
                // 获取水平和垂直输入
                m_horizontalInput = Input.GetAxisRaw("Horizontal");
                m_verticalInput = Input.GetAxisRaw("Vertical");

                // 检测冲刺输入
                CheckDashInput();
            }
            else
            {
                // 输入禁用时，重置输入值
                m_horizontalInput = 0;
                m_verticalInput = 0;
            }

            // 更新冲刺状态
            UpdateDashState();
        }

        private void FixedUpdate()
        {
            // 如果正在冲刺，应用冲刺移动
            if (m_isDashing)
            {
                DashMovement();
            }
            else
            {
                // 正常四方向移动
                Move();
            }
        }

        private void Move()
        {
            // 创建基于输入的移动向量
            Vector2 moveDirection = new Vector2(m_horizontalInput, m_verticalInput);

            // 归一化向量，确保对角线移动不会更快
            if (moveDirection.magnitude > 1)
            {
                moveDirection.Normalize();
            }

            // 应用移动速度
            Vector2 moveVelocity = moveDirection * m_moveSpeed;
            m_rigidbody2D.velocity = moveVelocity;
        }

        private void CheckDashInput()
        {
            // 如果可以冲刺且按下空格键
            if (m_canDash && Input.GetKeyDown(KeyCode.Space))
            {
                StartDash();
            }
        }

        private void StartDash()
        {
            m_isDashing = true;
            m_canDash = false;
            m_dashTimeLeft = m_dashDuration;
            m_dashCooldownTimeLeft = m_dashCooldown;

            // 获取当前移动方向作为冲刺方向
            m_dashDirection = new Vector2(m_horizontalInput, m_verticalInput);
            
            // 如果没有输入方向，默认向右冲刺
            if (m_dashDirection.magnitude <= 0.1f)
            {
                m_dashDirection = transform.right;
            }
            else
            {
                m_dashDirection.Normalize();
            }

            // 禁用伤害碰撞器
            if (m_dmgCollider != null)
            {
                m_dmgCollider.enabled = false;
            }
        }

        private void UpdateDashState()
        {
            if (m_isDashing)
            {
                m_dashTimeLeft -= Time.deltaTime;
                
                // 冲刺结束
                if (m_dashTimeLeft <= 0)
                {
                    EndDash();
                }
            }
            
            // 冲刺冷却计时
            if (!m_canDash)
            {
                m_dashCooldownTimeLeft -= Time.deltaTime;
                
                if (m_dashCooldownTimeLeft <= 0)
                {
                    m_canDash = true;
                }
            }
        }

        private void EndDash()
        {
            m_isDashing = false;
            
            // 重新启用伤害碰撞器
            if (m_dmgCollider != null)
            {
                m_dmgCollider.enabled = true;
            }
            
            // 冲刺结束后立即停止
            m_rigidbody2D.velocity = Vector2.zero;
        }

        private void DashMovement()
        {
            // 应用冲刺速度
            m_rigidbody2D.velocity = m_dashDirection * m_dashSpeed;
        }
    }
}
