using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //싱글톤 방식으로 오브젝트 공유
    public static GameManager gm_instance;

    public Vector3 gunEndPos;

    [Header("WeaponAssault")]
    [SerializeField]
    public WeaponAssaultRifle weapon;
    // Start is called before the first frame update

    private float attackDis;
    private float attackSpd;

    public GameObject player;
    public GameObject Head;
    public Rigidbody rb_player;
    public Rigidbody rb_weapon;
    public Collider gunCollider;

    public void getFromPC(GameObject player)
    {
        this.player = player;
        
        foreach(Transform child in this.player.transform)
        {
            if(child.gameObject.name == "Head")
            {
                Head = child.gameObject;
            }
        }

        this.rb_player = player.GetComponent<Rigidbody>();
        this.gunCollider = rb_player.GetComponent<Collider>();

        foreach(Transform child in player.transform)
        {
            if(child.tag == "Weapon")
            {
                rb_weapon = child.GetComponentInChildren<Rigidbody>();

                float[] receive = weapon.Gun_Info();

                this.attackDis = receive[0];
                this.attackSpd = receive[1];
                
                break;
            }
        }

        Renderer renderer = rb_weapon.GetComponent<Renderer>();
        float localZOffset = rb_weapon.transform.InverseTransformPoint(renderer.bounds.center).z;

        this.gunEndPos = rb_weapon.transform.position + rb_weapon.transform.forward * localZOffset;

        Debug.Log("총알 위치: " + this.gunEndPos);
    }


    void Awake()
    {
        bp = Bullet_Pool.bp_instance;
        gm_instance = GameManager.gm_instance;

        if (bc_instance == null )
        {
            bc_instance = this;
        }
        else
        {
            Debug.LogWarning("bc_Instance already exists, destroying object!");
        }

        gm_instance.AimPos(gunEndPos);
    }

    private void Update()
    {
        gm_instance.AimPos(gunEndPos);
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "bullet" && collision.gameObject.name != "Weapon")    //총알끼리 부딪혀서 사라지는 경우를 배제
        {
            Debug.Log("총알의 착탄: " + this.transform.position + " // " + collision.gameObject.name);

            bp.ReturnBullet(this.gameObject);
        }
            
    }
    public void Bullet_Shoot()   //그냥 총의 rigidBody는 받아라 그냥;;
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }

        weapon.UseAmmo();

        if (!weapon.canShoot) return;

        GameObject bullet = bp.GetBullet();

        bullet.gameObject.name = "kjih's bullet";   //날라가는 순간 이름 변경(플레이어 + bullet) 누군가의 총알인지 파악
        Debug.Log("Bullet name change: " + bullet.gameObject.name);

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        //화면 정중앙 좌표
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);


        if (Physics.Raycast(ray, out hit, 1000))
        {
            //물건이 감지됐을 경우 위치 파악
            targetPoint = hit.point;
        }
        else
        {   //10에 총의 사거리를 넣자
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        //방향을 플레이어가 바라보는 방향으로 조정
        Vector3 attackDirection = Head.transform.forward;

        // 총알의 위치를 총의 끝으로 설정
        bullet.transform.position = gunEndPos;

        // 총알의 방향 설정
        bullet.transform.rotation = Quaternion.LookRotation(attackDirection);

        // 총알에 힘을 가함
        bullet.GetComponent<Rigidbody>().velocity = attackDirection * attackSpd;

        if (Physics.Raycast(gunEndPos, attackDirection, out hit, 1000))
        {

        }

    }
}
