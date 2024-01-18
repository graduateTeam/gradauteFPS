using Mirror;
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

    public void get_Info(float attackDis, float attackSpd)
    {
        this.attackDis = attackDis;
        this.attackSpd = attackSpd;
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
            bp.ReturnBullet(this.gameObject);
        }
            
    }


    public void Bullet_Shoot(Rigidbody gun, Rigidbody player)
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }

        weapon.UseAmmo();

        if (!weapon.canShoot) return;

        GameObject bullet = bp.GetBullet();

        Collider collider = gun.GetComponent<Collider>();
        float gunLength = collider.bounds.size.z;
        Vector3 gunEndPos = gun.transform.position + gun.transform.rotation * Vector3.forward * gunLength;

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        //화면 정중앙 좌표
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(player.transform.position, gun.transform.rotation * oldray.direction);


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

        Vector3 attackDirection = (targetPoint - gunEndPos).normalized;
        attackDirection = Vector3.Scale(attackDirection, new Vector3(-1, -1, -1));



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
