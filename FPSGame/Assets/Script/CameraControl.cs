using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float MouseSen = 40f;

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

        MouseY = Mathf.Clamp(MouseY, -75f, 75f);    //�� �Ʒ� �� �ִ� ���� -75 ~ 75

        transform.localRotation = Quaternion.Euler(MouseX, MouseY, 0f);
    }
}
