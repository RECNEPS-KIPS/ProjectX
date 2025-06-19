using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_CameraController_Pus : MonoBehaviour
{
    ///   <summary>
    ///   </summary>
    public Transform player;

    Vector3[] v3;
    public int num;
    public Vector3 start;
    public Vector3 end;
    Vector3 tagetPostion;
    Vector3 ve3;
    Quaternion angel;
    public float speed;

    void Start()
    {
        v3 = new Vector3[num];
    }

    void LateUpdate()
    {
        start = player.position + player.up * 2.0f - player.forward * 3.0f;
        end = player.position + player.up * 5.0f;
        if (Input.GetMouseButton(1))
        {
            Vector3 pos = transform.position;
            Vector3 rot = transform.eulerAngles;
            transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * 10);
            transform.RotateAround(transform.position, Vector3.left, -Input.GetAxis("Mouse Y") * 10);
            if (transform.eulerAngles.x < -60 || transform.eulerAngles.x > 60)
            {
                transform.position = pos;
                transform.eulerAngles = rot;
            }

            return;
        }

        tagetPostion = start;
        v3[0] = start;
        v3[num - 1] = end;
        for (int i = 1; i < num; i++)
        {
            v3[i] = Vector3.Lerp(start, end, i / num);
        }

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

        transform.position = Vector3.SmoothDamp(transform.position, tagetPostion, ref ve3, 0);
        angel = Quaternion.LookRotation(player.position - tagetPostion);
        transform.rotation = Quaternion.Slerp(transform.rotation, angel, speed);
    }

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