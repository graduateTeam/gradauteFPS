using Mirror;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Control : NetworkBehaviour
{
    public int Data { get; set; }

    [SerializeField]
    private Bullet_Pool bp;

    public static Bullet_Control instance;
    public static GameManager gm_instance;

    [SyncVar]
    public Vector3 gunEndPos;

    [Header("WeaponAssault")]
    [SerializeField]
    public WeaponAssaultRifle weapon;
    // Start is called before the first frame update

    [SyncVar]
    private float attackDis;

    [SyncVar]
    private float attackSpd;

    public GameObject Head;
    public Rigidbody rb_weapon;

    public void setWeaponInfo(GameObject Head, WeaponAssaultRifle weapon)
    {
        this.Head = Head;
        this.weapon = weapon; // WeaponAssaultRifle 컴포넌트 저장

        if (weapon != null)
        {
            rb_weapon = weapon.GetComponentInChildren<Rigidbody>();

            float[] receive = weapon.Gun_Info();
            attackDis = receive[0];
            attackSpd = receive[1];

            Renderer renderer = rb_weapon.GetComponent<Renderer>();
            float localZOffset = rb_weapon.transform.InverseTransformPoint(renderer.bounds.center).z;

            gunEndPos = rb_weapon.transform.position + rb_weapon.transform.forward * localZOffset;
        }
        else
        {
            Debug.LogError("WeaponAssaultRifle is null");
        }
    }

    /*public void getFromPC(GameObject player)
    {
        foreach (Transform child in player.transform)
        {
            if (child.gameObject.name == "Head")
            {
                Head = child.gameObject;
            }
        }

        Rigidbody rb_player = player.GetComponent<Rigidbody>();
        Collider gunCollider = rb_player.GetComponent<Collider>();

        foreach (Transform child in player.transform)
        {
            if (child.tag == "Weapon")
            {
                rb_weapon = child.GetComponentInChildren<Rigidbody>();

                float[] receive = weapon.Gun_Info();

                attackDis = receive[0];
                attackSpd = receive[1];

                break;
            }
        }

        Renderer renderer = rb_weapon.GetComponent<Renderer>();
        float localZOffset = rb_weapon.transform.InverseTransformPoint(renderer.bounds.center).z;

        gunEndPos = rb_weapon.transform.position + rb_weapon.transform.forward * localZOffset;
    }*/

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임 매니저가 씬 전환 시 파괴되지 않도록 함
        }
    }

    private void Start()
    {
        bp = Bullet_Pool.instance;
        gm_instance = GameManager.instance;

        gm_instance.AimPos(gunEndPos);

        if (bp == null)
            Debug.LogError("Bullet_Controll bp is null");
    }

    private void Update()
    {
        gm_instance.AimPos(gunEndPos);
    }
   
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "bullet" && collision.gameObject.name != "Weapon")
        {
            bp.ReturnBullet(this.gameObject);
        }

    }

    /*public void Bullet_Shoot()
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }
        weapon.UseAmmo();

        if (!weapon.canShoot) return ;

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);


        if (Physics.Raycast(ray, out hit, 1000))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        Vector3 attackDirection = Head.transform.forward;

        var LookDirection = Quaternion.LookRotation(attackDirection);

        Vector3 velo = attackDirection * attackSpd;

        if (bp != null)
        {
            var bullet = bp.GetBullet(gunEndPos, LookDirection, velo);
            // 생성된 총알 정보를 클라이언트에게 동기화 추가된 부분 0410
            StartCoroutine(SyncBulletAfterSpawn(bullet, LookDirection, velo));
        }
        else
        {
            Debug.LogError("bp is null in Bullet_Shoot");
        }

        if (Physics.Raycast(gunEndPos, attackDirection, out hit, 1000))
        {

        }

    }*/
   /* public GameObject Bullet_Shoot()    //0418 bp의 부재를 해결해야한다.
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return null;
        }

        //weapon.UseAmmo();
        if (!weapon.canShoot) return null;

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);

        if (Physics.Raycast(ray, out hit, 1000))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        Vector3 attackDirection = Head.transform.forward;
        var LookDirection = Quaternion.LookRotation(attackDirection);
        Vector3 velo = attackDirection * attackSpd;

        GameObject returnValue = null;

        try
        {
            var bullet = bp.GetBullet(gunEndPos, LookDirection, velo);
            SyncBulletAfterSpawn(bullet, LookDirection, velo);
            returnValue = bullet;

        }catch(Exception ex)
        {
            Debug.LogException(ex);

            if(bp == null)
            {
                Debug.LogError("bp is null in Bullet_Shoot");
                returnValue = null;
            }

        }

        return returnValue;
    }*/

    private void SyncBulletAfterSpawn(GameObject bullet, Quaternion LookDirection, Vector3 velo)
    {
        NetworkIdentity bulletIdentity = bullet.GetComponent<NetworkIdentity>();

        if (bullet != null)
        {
            GameObject playerObject = OSOPRoomManager.singleton.playerPrefab;
            if (playerObject != null)
            {
                Player_Control pc = playerObject.GetComponent<Player_Control>();
                if (pc != null)
                {
                    pc.RpcSyncBullet(bulletIdentity.netId, gunEndPos, LookDirection, velo);
                }
                else
                {
                    Debug.LogError("Player_Control component not found on playerPrefab");
                }
            }
            else
            {
                Debug.LogError("playerPrefab is null in OSOPRoomManager.singleton");
            }
        }
    }

    public void setPlayerControl(Player_Control pc)
    {
        pc.setBulletControl(this);
    }

    public void setGameManager()
    {
        gm_instance = GameManager.instance;
    }

    public void setBulletPool()
    {
        bp = Bullet_Pool.instance;
    }
}