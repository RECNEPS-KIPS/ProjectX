using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_CameraController_Pus : MonoBehaviour
{

    ///   <summary>
    ///  相机进行射线检测，如果检测不到主角，
    ///  就在起始点与结束点之间寻找几个点，
    ///  直到找到可以看到主角的点
    /// 在游戏中玩家可以用鼠标控制相机的旋转
    ///   </summary>

    
        public Transform player;     //角色位置信息
        Vector3[] v3;         //相机自动找寻的位置点
        public int num;              //相机临时点的个数
        public Vector3 start;        //相机开始时的位置
        public Vector3 end;          //相机没有找到主角时的位置
        Vector3 tagetPostion;        //相机看向的目标点
        Vector3 ve3;                 //平滑阻尼的ref参数
        Quaternion angel;            //相机看向目标的旋转值
        public float speed;          //相机移动速度
        void Start()
        {
            //外界赋值数组长度
            v3 = new Vector3[num];
        }
        void LateUpdate()
        {
            //记录相机初始位置
            start = player.position + player.up * 2.0f - player.forward * 3.0f;
            //记录相机最终位置
            end = player.position + player.up * 5.0f;
            //鼠标控制相机的旋转
            if (Input.GetMouseButton(1))
            {
                //记录相机的初始位置和旋转角度
                Vector3 pos = transform.position;
                Vector3 rot = transform.eulerAngles;
                //让相机绕着指定轴向旋转
                transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * 10);
                transform.RotateAround(transform.position, Vector3.left, -Input.GetAxis("Mouse Y") * 10);
                //限制相机的绕X旋转的角度
                if (transform.eulerAngles.x < -60 || transform.eulerAngles.x > 60)
                {
                    transform.position = pos;
                    transform.eulerAngles = rot;
                }
                return;
            }
            //相机目标位置，开始等于初始位置
            tagetPostion = start;
            v3[0] = start;
            v3[num - 1] = end;
            //动态获取相机的几个点
            for (int i = 1; i < num; i++)
            {
                v3[i] = Vector3.Lerp(start, end, i / num);
            }
            //判断相机在那个点可以看到主角
            for (int i = 0; i < num; i++)
            {
                if (Function(v3[i]))
                {
                    tagetPostion = v3[i];
                    break;
                }
                if (i == num - 1)
                {
                    tagetPostion = end;
                }
            }
            //主角的移动和看向
            transform.position = Vector3.SmoothDamp(transform.position, tagetPostion, ref ve3, 0);
            angel = Quaternion.LookRotation(player.position - tagetPostion);
            transform.rotation = Quaternion.Slerp(transform.rotation, angel, speed);
        }
        ///   <summary>
        ///  射线检测，相机是否能照到主角
        ///   </summary>
        ///   <param name=" v3 "> 计算射线发射的方向 </param>
        ///   <returns> 是否检测到 </returns>
        bool Function(Vector3 v3)
        {
            RaycastHit hit;
            if (Physics.Raycast(v3, player.position - v3, out hit))
            {
                if (hit.collider.tag == "Player")
                {
                    return true;
                }
            }
            return false;
        }
    }
 




