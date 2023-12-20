using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //싱글톤 방식으로 오브젝트 공유
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
            //Destroy(this);    //해제 시 처음에 비활성화 되어있는 총알 특성상 스크립트가 인스펙터 창에서 사라질 수 있다.
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "bullet")    //총알끼리 부딪혀서 사라지는 경우를 배제
            bp.ReturnBullet(this.gameObject);
    }

    public void Bullet_Shoot(Rigidbody gun)
    {
        GameObject bullet = bp.GetBullet();

        Collider collider = gun.GetComponent<Collider>();
        float gunLength = collider.bounds.size.z;

        Vector3 b_vec = gun.transform.forward;  //실제 앞부분과 차이가 생기는데 움직이고 나면?
        bullet.transform.position = gun.transform.position + (b_vec * gunLength * 2);

        bullet.transform.rotation = Quaternion.LookRotation(b_vec);
        bullet.GetComponent<Rigidbody>().AddForce(b_vec * 100);
    }
}
