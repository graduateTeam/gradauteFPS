using Mirror;
using System;
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

    public GameObject player;
    public GameObject Head;
    public Rigidbody rb_weapon;

   public override void OnStartServer()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임 매니저가 씬 전환 시 파괴되지 않도록 함
        }

        bp = Bullet_Pool.instance;
        gm_instance = GameManager.instance;

        gm_instance.AimPos(gunEndPos);
    }

    public override void OnStartClient()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 게임 매니저가 씬 전환 시 파괴되지 않도록 함
        }

        bp = Bullet_Pool.instance;
        gm_instance = GameManager.instance;

        gm_instance.AimPos(gunEndPos);
    }

    public void getFromPC(GameObject player)
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
    }

    private void Awake()
    {
        //playerPrefab.GetComponent<Player_Control>().getBC(GetComponent<Bullet_Control>());
    }

    /*void Awake()
    {
        if (isServer)
            OnStartServer();
        else
            OnStartClient();

        bp = Bullet_Pool.instance;
        gm_instance = GameManager.instance;

        gm_instance.AimPos(gunEndPos);

    }*/

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

    public void Bullet_Shoot()
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }
        weapon.UseAmmo();

        if (!weapon.canShoot) return;

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

        bp.GetBullet(gunEndPos, LookDirection, velo);

        if (Physics.Raycast(gunEndPos, attackDirection, out hit, 1000))
        {

        }

    }

}