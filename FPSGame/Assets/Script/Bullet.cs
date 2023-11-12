using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        bullet = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player_Control player = collision.gameObject.GetComponent<Player_Control>();

        if(player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //플레이어가 서 있는 위치
            Vector3 bullet_pos = bullet.transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //임의로 대미지 10이라고 한 것 스크립트에 대미지가 정해지면 그 대미지로 바꿀 것
        }

        Bullet_Pool pool = GetComponent<Bullet_Pool>();
        pool.ReturnBullet(bullet);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100-dis) / 100),2)); //임의의 계산 제대로 몸의 중심으로부터 거리에 따른 대미지 경감이 들어갈지 미지수
    }
}
