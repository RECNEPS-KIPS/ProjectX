using UnityEngine;

public class CharacterEasyMove : MonoBehaviour
{
    private Transform Cam;
    CharacterController controller;
    [SerializeField] private float speed;
    private void Awake()
    {
        Cam = Camera.main.transform;
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        CharacterMove();
    }
    private void LateUpdate()
    {
        if (CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero)
        {
          //  CharacterRotation();
            CharacterRotation();
        }

    }

    //private Vector3 InputDir()
    //{
    //    Vector3 camForwardDir =new Vector3(Cam.forward.x, 0, Cam.forward.z).normalized;
    //    return  (camForwardDir * CharacterInputSystem.MainInstance.PlayerMove.y + Cam.right * CharacterInputSystem.MainInstance.PlayerMove.x).normalized;
    //}
    //private void CharacterRotation()
    //{
    //     float targetAngle = Mathf.Atan2(InputDir().x, InputDir().z) * Mathf.Rad2Deg;
    //     transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0),Time.deltaTime*speed);
    //}
    private void CharacterMove()
    {
        controller.Move(transform.forward * (CharacterInputSystem.MainInstance.PlayerMove != Vector2.zero ? 0.08f : 0));
    }
    Vector3 targetDirection;
    private void CharacterRotation()
    {
        if (CharacterInputSystem.MainInstance.PlayerMove == Vector2.zero) { return; }

        // 获取摄像机的前向向量在水平平面上的投影
        Vector3 camForward = new Vector3(Cam.forward.x, 0, Cam.forward.z).normalized;

        // 计算玩家输入方向
        float targetAngle = Mathf.Atan2(CharacterInputSystem.MainInstance.PlayerMove.x, CharacterInputSystem.MainInstance.PlayerMove.y) * Mathf.Rad2Deg;

        // 计算目标方向
        targetDirection = Quaternion.Euler(0, targetAngle, 0) * camForward;

        // 平滑地旋转角色到目标方向
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,(transform.position+targetDirection*5));
    }
}
