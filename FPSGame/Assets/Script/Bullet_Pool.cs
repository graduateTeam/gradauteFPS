using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.ComponentModel;
using System;

/*총의 오브젝트 풀이 생성되지 않는 것이 문제*/
public class Bullet_Pool : NetworkBehaviour
{

    // 생성할 총알의 원본 프리팹
    public GameObject bulletPrefab;

    // 풀의 크기 (미리 생성할 총알 수)
    public int poolSize = 100;
    // Bullet_Pool 클래스의 인스턴스를 저장하는 정적 변수
    public static Bullet_Pool _instance;
    public static Bullet_Pool instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<Bullet_Pool>();
            }

            return _instance;
        }
    }

    private Queue<GameObject> bulletPool;

    public override void OnStartServer()
    {
        pool_spawn();
    }
    // 풀에서 사용 가능한 총알을 가져오는 함수
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else
        {   
            //없으면 새로 생성
            GameObject bullet = Instantiate(bulletPrefab);
            NetworkServer.Spawn(bullet);
            return bullet;
        }
    }

    // 총알을 풀에 반환하는 함수
    public void ReturnBullet(GameObject bullet)
    {
        // 총알을 비활성화
        bullet.SetActive(false);
    }

    private void pool_spawn()
    {
        bulletPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            NetworkServer.Spawn(bullet);
            bullet.SetActive(false); // 총알을 비활성화
            bulletPool.Enqueue(bullet);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player_Control player = collision.gameObject.GetComponent<Player_Control>();

        if (player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //플레이어가 서 있는 위치
            Vector3 bullet_pos = bulletPrefab.transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //임의로 대미지 10이라고 한 것 스크립트에 대미지가 정해지면 그 대미지로 바꿀 것
        }

        ReturnBullet(bulletPrefab);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //임의의 계산 제대로 몸의 중심으로부터 거리에 따른 대미지 경감이 들어갈지 미지수
    }

    public void MoveBullet(Vector3 vec)
    {
        bulletPrefab.GetComponent<Rigidbody>().AddForce(vec);
    }
}