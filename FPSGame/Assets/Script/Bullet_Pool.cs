using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.ComponentModel;
using System;

/*���� ������Ʈ Ǯ�� �������� �ʴ� ���� ����*/
public class Bullet_Pool : NetworkBehaviour
{

    // ������ �Ѿ��� ���� ������
    public GameObject bulletPrefab;

    // Ǯ�� ũ�� (�̸� ������ �Ѿ� ��)
    public int poolSize = 100;
    // Bullet_Pool Ŭ������ �ν��Ͻ��� �����ϴ� ���� ����
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
    // Ǯ���� ��� ������ �Ѿ��� �������� �Լ�
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
            //������ ���� ����
            GameObject bullet = Instantiate(bulletPrefab);
            NetworkServer.Spawn(bullet);
            return bullet;
        }
    }

    // �Ѿ��� Ǯ�� ��ȯ�ϴ� �Լ�
    public void ReturnBullet(GameObject bullet)
    {
        // �Ѿ��� ��Ȱ��ȭ
        bullet.SetActive(false);
    }

    private void pool_spawn()
    {
        bulletPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            NetworkServer.Spawn(bullet);
            bullet.SetActive(false); // �Ѿ��� ��Ȱ��ȭ
            bulletPool.Enqueue(bullet);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player_Control player = collision.gameObject.GetComponent<Player_Control>();

        if (player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //�÷��̾ �� �ִ� ��ġ
            Vector3 bullet_pos = bulletPrefab.transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //���Ƿ� ����� 10�̶�� �� �� ��ũ��Ʈ�� ������� �������� �� ������� �ٲ� ��
        }

        ReturnBullet(bulletPrefab);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //������ ��� ����� ���� �߽����κ��� �Ÿ��� ���� ����� �氨�� ���� ������
    }

    public void MoveBullet(Vector3 vec)
    {
        bulletPrefab.GetComponent<Rigidbody>().AddForce(vec);
    }
}