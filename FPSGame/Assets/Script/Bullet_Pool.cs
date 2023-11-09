using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Pool : MonoBehaviour
{
    // Bullet_Pool Ŭ������ �ν��Ͻ��� �����ϴ� ���� ����
    public static Bullet_Pool instance;

    // ������ �Ѿ��� ���� ������
    public GameObject bulletPrefab;

    // Ǯ�� ũ�� (�̸� ������ �Ѿ� ��)
    public int poolSize = 100;

    // Ǯ���� �Ѿ��� �����ϴ� ����Ʈ
    private List<GameObject> bulletPool;

    private void Awake()
    {
        // ���� �ν��Ͻ��� ���� ������ ����
        instance = this;

        // �Ѿ� Ǯ�� �ʱ�ȭ
        bulletPool = new List<GameObject>();

        // ������ Ǯ ũ�⸸ŭ �Ѿ��� �̸� ����
        for (int i = 0; i < poolSize; i++)
        {
            // �Ѿ� �������� �ν��Ͻ�ȭ�Ͽ� �� �Ѿ��� ����
            GameObject bullet = Instantiate(bulletPrefab);

            // �� �Ѿ��� ��Ȱ��ȭ
            bullet.SetActive(false);

            // �Ѿ� Ǯ�� �� �Ѿ��� �߰�
            bulletPool.Add(bullet);
        }
    }

    // Ǯ���� ��� ������ �Ѿ��� �������� �Լ�
    public GameObject GetBullet()
    {
        // Ǯ�� ��� �Ѿ��� �˻�
        foreach (GameObject bullet in bulletPool)
        {
            // ��Ȱ��ȭ�� �Ѿ��� ã����
            if (!bullet.activeInHierarchy)
            {
                // �Ѿ��� Ȱ��ȭ�ϰ� ��ȯ
                bullet.SetActive(true);
                return bullet;
            }
        }

        // ��� ������ �Ѿ��� ������ null ��ȯ
        return null;
    }

    // �Ѿ��� Ǯ�� ��ȯ�ϴ� �Լ�
    public void ReturnBullet(GameObject bullet)
    {
        // �Ѿ��� ��Ȱ��ȭ
        bullet.SetActive(false);
    }
}