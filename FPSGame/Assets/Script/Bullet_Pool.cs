using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*���� ������Ʈ Ǯ�� �������� �ʴ� ���� ����*/
public class Bullet_Pool : NetworkBehaviour
{
    // ������ �Ѿ��� ���� ������
    public GameObject bulletPrefab;

    // Ǯ�� ũ�� (�̸� ������ �Ѿ� ��)
    [SyncVar]
    public int poolSize = 100;
    // Bullet_Pool Ŭ������ �ν��Ͻ��� �����ϴ� ���� ����
    public static Bullet_Pool instance; //�̱��� ������� ������Ʈ ����

    private Queue<GameObject> bulletPool;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ���� �Ŵ����� �� ��ȯ �� �ı����� �ʵ��� ��
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

            // ������Ʈ ��������
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


    // �Ѿ��� ��Ȱ��ȭ�ϰ� Ǯ�� ��ȯ�ϴ� �Լ�
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
            bullet.SetActive(false); // �Ѿ��� ��Ȱ��ȭ ���·� ����ϴ�.
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