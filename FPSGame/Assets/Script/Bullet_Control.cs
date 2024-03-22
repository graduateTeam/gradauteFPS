using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //ï¿½Ì±ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Æ® ï¿½ï¿½ï¿½ï¿½
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

    public void getFromPC(GameObject player)    //°ªÀ» ¹Þ¾Æ¿Í ÀûÀýÇÑ À§Ä¡ ¼±Á¤±îÁö
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

        Debug.Log("ï¿½Ñ¾ï¿½ ï¿½ï¿½Ä¡: " + this.gunEndPos);
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
        if (collision.gameObject.tag != "bullet" && collision.gameObject.name != "Weapon")    //ï¿½Ñ¾Ë³ï¿½ï¿½ï¿½ ï¿½Îµï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿?ï¿½ï¿½ì¸?ï¿½ï¿½ï¿½ï¿½
        {
            Debug.Log("ï¿½Ñ¾ï¿½ï¿½ï¿½ ï¿½ï¿½Åº: " + this.transform.position + " // " + collision.gameObject.name);

            bp.ReturnBullet(this.gameObject);
        }
            
    }

    public void Bullet_Shoot()   //ÃÑ¾Ë ¹ß»ç·ÎÁ÷
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }

        weapon.UseAmmo();

        if (!weapon.canShoot) return;

        GameObject bullet = bp.GetBullet();

        bullet.gameObject.name = "kjih's bullet";   //ï¿½ï¿½ï¿½ó°¡´ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ì¸ï¿½ ï¿½ï¿½ï¿½ï¿½(ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ + bullet) ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ñ¾ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ä¾ï¿½
        Debug.Log("Bullet name change: " + bullet.gameObject.name);

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        //È­ï¿½ï¿½ ï¿½ï¿½ï¿½ß¾ï¿½ ï¿½ï¿½Ç¥
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);


        if (Physics.Raycast(ray, out hit, 1000))
        {
            //ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿?ï¿½ï¿½Ä¡ ï¿½Ä¾ï¿½
            targetPoint = hit.point;
        }
        else
        {   //10ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½Å¸ï¿½ï¿½ï¿?ï¿½ï¿½ï¿½ï¿½
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        //ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ã·ï¿½ï¿½Ì¾î°¡ ï¿½Ù¶óº¸´ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        Vector3 attackDirection = Head.transform.forward;

        // ï¿½Ñ¾ï¿½ï¿½ï¿½ ï¿½ï¿½Ä¡ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        bullet.transform.position = gunEndPos;

        // ï¿½Ñ¾ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        bullet.transform.rotation = Quaternion.LookRotation(attackDirection);

        // ï¿½Ñ¾Ë¿ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        bullet.GetComponent<Rigidbody>().velocity = attackDirection * attackSpd;

        if (Physics.Raycast(gunEndPos, attackDirection, out hit, 1000))
        {

        }

    }
}
