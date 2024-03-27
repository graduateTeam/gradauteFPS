using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
            Debug.Log("Bullet_Pool instance activated");
            // �� ��ȯ �� �ı����� �ʵ��� ���� (���û���)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Another Bullet_Pool instance found. Destroying...");
        }

        bulletPool = new Queue<GameObject>();
    }

    //[Command]
    public void GetBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true); // �Ѿ��� Ȱ��ȭ
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.GetComponent<Rigidbody>().velocity = velocity;


            NetworkServer.Spawn(bullet);
            //bulletPool.Enqueue(bullet); // Put it back for reuse
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

}