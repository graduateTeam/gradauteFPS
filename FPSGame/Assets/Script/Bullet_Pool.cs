using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*���� ������Ʈ Ǯ�� �������� �ʴ� ���� ����*/
public class Bullet_Pool : NetworkBehaviour
{

    // ������ �Ѿ��� ���� ������
    public GameObject bulletPrefab;

    // Ǯ�� ũ�� (�̸� ������ �Ѿ� ��)
    public int poolSize = 100;
    // Bullet_Pool Ŭ������ �ν��Ͻ��� �����ϴ� ���� ����
    public static Bullet_Pool bp_instance; //�̱��� ������� ������Ʈ ����

    void Awake()
    {
        if (bp_instance == null)
        {
            bp_instance = this;
        }
        else
        {
            Debug.LogWarning("bp_Instance already exists, destroying object!");
            Destroy(this);
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
        bullet.gameObject.name = "bullet";  //��Ȱ��ȭ �Ǵ� ���� �÷��̾��� �Ѿ˿��� �׳� �Ѿ˷� ��ȯ
        Debug.Log("Bullet name �������: " + bullet.gameObject.name);
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
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
}