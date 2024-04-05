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

    private void SpawnBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.GetComponent<Rigidbody>().velocity = velocity;
        }
    }

    [ClientRpc]
    public void RpcSpawnBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        SpawnBullet(pos, rotation, velocity);
    }

    public void GetBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (isServer)
        {
            SpawnBullet(pos, rotation, velocity);
            NetworkServer.Spawn(bulletPool.Peek());
            RpcSpawnBullet(pos, rotation, velocity);
        }
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
}