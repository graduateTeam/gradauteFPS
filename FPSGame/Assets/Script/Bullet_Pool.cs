using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
            Debug.Log("Bullet_Pool instance activated");
            // 씬 전환 시 파괴되지 않도록 설정 (선택사항)
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
            bullet.SetActive(true); // 총알을 활성화
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.GetComponent<Rigidbody>().velocity = velocity;


            NetworkServer.Spawn(bullet);
            //bulletPool.Enqueue(bullet); // Put it back for reuse
        }
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

            //NetworkServer.Spawn(bullet);

            bulletPool.Enqueue(bullet);
            bullet.SetActive(false); // 총알을 비활성화 상태로 만듭니다.
        }
    }

}