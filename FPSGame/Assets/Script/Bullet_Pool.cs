using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*총의 오브젝트 풀이 생성되지 않는 것이 문제*/
public class Bullet_Pool : NetworkBehaviour
{
    // 생성할 총알의 원본 프리팹
    public GameObject bulletPrefab;

    // 풀의 크기 (미리 생성할 총알 수)
    [SyncVar]
    public int poolSize = 100;
    // Bullet_Pool 클래스의 인스턴스를 저장하는 정적 변수
    public static Bullet_Pool instance; //싱글톤 방식으로 오브젝트 공유

    private Queue<GameObject> bulletPool;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임 매니저가 씬 전환 시 파괴되지 않도록 함
        }

        bulletPool = new Queue<GameObject>();
        pool_spawn();
    }

    /*private void SpawnBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);

            // 컴포넌트 가져오기
            var bulletControl = bullet.GetComponent<Bullet_Control>();
            var pc = OSOPRoomManager.singleton.playerPrefab.GetComponent<Player_Control>();
            if (bulletControl != null)
            {
                bulletControl.setBulletPool();
                bulletControl.setWeaponInfo(pc.Head, pc.AssaultRifle);
            }
            else
            {
                Debug.Log("Bullet_Control is null");
            }

            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

    [ClientRpc]
    public void RpcSpawnBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        SpawnBullet(pos, rotation, velocity);
    }*/

    /*public void GetBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (isServer)
        {
            SpawnBullet(pos, rotation, velocity);
            NetworkServer.Spawn(bulletPool.Peek());
            RpcSpawnBullet(pos, rotation, velocity);
        }
    }*/
    public GameObject GetBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (isServer)
        {
            if (bulletPool.Count > 0)
            {
                GameObject bullet = bulletPool.Dequeue();
                bullet.SetActive(true);
                bullet.transform.position = pos;
                bullet.transform.rotation = rotation;
                bullet.GetComponent<Rigidbody>().velocity = velocity;

                /*NetworkIdentity bulletIdentity = bullet.GetComponent<NetworkIdentity>();
                if (!bulletIdentity.isServer && !bulletIdentity.isClient)
                {
                    NetworkServer.Spawn(bullet);
                }*/
                //RpcSpawnBullet(bullet.GetComponent<NetworkIdentity>().netId, pos, rotation, velocity);

                return bullet;
            }
        }

        return null;
    }


    // 총알을 비활성화하고 풀로 반환하는 함수
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    public void pool_spawn()
    {
        for (int i = 0; i < poolSize; i++)
        {      
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(true);

            Bullet_Control bulletControl = bullet.GetComponent<Bullet_Control>();

            if (bulletControl != null)
            {
                bulletControl.setBulletPool(this);
                bulletControl.setGameManager();
            }
            else
            {
                Debug.LogError("Bullet_Control component not found on bullet prefab");
            }

            //NetworkServer.Spawn(bullet);

            bulletPool.Enqueue(bullet);
            bullet.SetActive(false); // 총알을 비활성화 상태로 만듭니다.
        }
    }

    public bool Bp_isEmpty()
    {
        if (bulletPool == null) return true;

        return false;
    }

    public void setPlayerControl(Player_Control pc)
    {
        pc.setBulletPool(this);
    }
}