using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //�̱��� ������� ������Ʈ ����
    // Start is called before the first frame update
    void Awake()
    {
        bp = Bullet_Pool.bp_instance;

        if(bc_instance == null )
        {
            bc_instance = this;
        }
        else
        {
            Debug.LogWarning("bc_Instance already exists, destroying object!");
            //Destroy(this);    //���� �� ó���� ��Ȱ��ȭ �Ǿ��ִ� �Ѿ� Ư���� ��ũ��Ʈ�� �ν����� â���� ����� �� �ִ�.
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "bullet")    //�Ѿ˳��� �ε����� ������� ��츦 ����
            bp.ReturnBullet(this.gameObject);
    }

    public void Bullet_Shoot(Rigidbody gun)
    {
        GameObject bullet = bp.GetBullet();

        Collider collider = gun.GetComponent<Collider>();
        float gunLength = collider.bounds.size.z;

        Vector3 b_vec = gun.transform.forward;  //���� �պκа� ���̰� ����µ� �����̰� ����?
        bullet.transform.position = gun.transform.position + (b_vec * gunLength * 2);

        bullet.transform.rotation = Quaternion.LookRotation(b_vec);
        bullet.GetComponent<Rigidbody>().AddForce(b_vec * 100);
    }
}
