using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float MouseSen = 1500f;

    public float MouseX;
    public float MouseY;

    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

        MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

        MouseY = Mathf.Clamp(MouseY, -90f, 90f);    //위 아래 고개 최대 범위 -75 ~ 75

        Quaternion quat = Quaternion.Euler(new Vector3(MouseY, -MouseX, 0));
        transform.rotation
            = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime * MouseSen);
    }
}
