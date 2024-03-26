using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*총의 오브젝트 풀이 생성되지 않는 것이 문제*/
public class Bullet_Pool : NetworkBehaviour
{

    // 생성할 총알의 원본 프리팹
    public GameObject bulletPrefab;

    // 풀의 크기 (미리 생성할 총알 수)
    public int poolSize = 100;
    // Bullet_Pool 클래스의 인스턴스를 저장하는 정적 변수
    public static Bullet_Pool instance; //싱글톤 방식으로 오브젝트 공유

    private Queue<GameObject> bulletPool;

    public override void OnStartServer()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("Bullet_Pool instance activate Server!");
        }
        else
        {
            Debug.Log("Bullet_Pool instance Already exist Server!");
        }

        pool_spawn();
    }

    public override void OnStartClient()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("Bullet_Pool instance activate Client!");
        }
        else
        {
            Debug.Log("Bullet_Pool instance Already exist Client!");
        }

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
        bullet.gameObject.name = "bullet";  //비활성화 되는 순간 플레이어의 총알에서 그냥 총알로 변환
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    private void pool_spawn()
    {
        bulletPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);

            if(NetworkServer.active)
                NetworkServer.Spawn(bullet);

            bullet.SetActive(false); // 총알을 비활성화
            bulletPool.Enqueue(bullet);
        }
    }
}