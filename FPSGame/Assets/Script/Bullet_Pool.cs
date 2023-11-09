using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Pool : MonoBehaviour
{
    // Bullet_Pool 클래스의 인스턴스를 저장하는 정적 변수
    public static Bullet_Pool instance;

    // 생성할 총알의 원본 프리팹
    public GameObject bulletPrefab;

    // 풀의 크기 (미리 생성할 총알 수)
    public int poolSize = 100;

    // 풀링된 총알을 저장하는 리스트
    private List<GameObject> bulletPool;

    private void Awake()
    {
        // 현재 인스턴스를 정적 변수에 저장
        instance = this;

        // 총알 풀을 초기화
        bulletPool = new List<GameObject>();

        // 지정된 풀 크기만큼 총알을 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            // 총알 프리팹을 인스턴스화하여 새 총알을 생성
            GameObject bullet = Instantiate(bulletPrefab);

            // 새 총알을 비활성화
            bullet.SetActive(false);

            // 총알 풀에 새 총알을 추가
            bulletPool.Add(bullet);
        }
    }

    // 풀에서 사용 가능한 총알을 가져오는 함수
    public GameObject GetBullet()
    {
        // 풀의 모든 총알을 검사
        foreach (GameObject bullet in bulletPool)
        {
            // 비활성화된 총알을 찾으면
            if (!bullet.activeInHierarchy)
            {
                // 총알을 활성화하고 반환
                bullet.SetActive(true);
                return bullet;
            }
        }

        // 사용 가능한 총알이 없으면 null 반환
        return null;
    }

    // 총알을 풀에 반환하는 함수
    public void ReturnBullet(GameObject bullet)
    {
        // 총알을 비활성화
        bullet.SetActive(false);
    }
}