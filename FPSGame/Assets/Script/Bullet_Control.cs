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
    public Player_Control player;
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
        Debug.Log("총알의 충돌");
        if (player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //플레이어가 서 있는 위치
            Vector3 bullet_pos = transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //임의로 대미지 10이라고 한 것 스크립트에 대미지가 정해지면 그 대미지로 바꿀 것
        }

        if (collision.gameObject.tag != "bullet")    //총알끼리 부딪혀서 사라지는 경우를 배제
            bp.ReturnBullet(this.gameObject);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //임의의 계산 제대로 몸의 중심으로부터 거리에 따른 대미지 경감이 들어갈지 미지수
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
