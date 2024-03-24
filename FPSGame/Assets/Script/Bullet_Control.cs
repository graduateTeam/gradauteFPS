using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control instance;   //�̱��� ������� ������Ʈ ����
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

        Debug.Log("�Ѿ� ��ġ: " + this.gunEndPos);
    }


    void Awake()
    {
        bp = Bullet_Pool.bp_instance;
        gm_instance = GameManager.instance;

        if (instance == null )
        {
            instance = this;
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
        if (collision.gameObject.tag != "bullet" && collision.gameObject.name != "Weapon")    //�Ѿ˳��� �ε����� ������� ��츦 ����
        {
            Debug.Log("�Ѿ��� ��ź: " + this.transform.position + " // " + collision.gameObject.name);

            bp.ReturnBullet(this.gameObject);
        }
            
    }
    public void Bullet_Shoot()   //�׳� ���� rigidBody�� �޾ƶ� �׳�;;
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }

        weapon.UseAmmo();

        if (!weapon.canShoot) return;

        GameObject bullet = bp.GetBullet();

        bullet.gameObject.name = "kjih's bullet";   //���󰡴� ���� �̸� ����(�÷��̾� + bullet) �������� �Ѿ����� �ľ�
        Debug.Log("Bullet name change: " + bullet.gameObject.name);

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        //ȭ�� ���߾� ��ǥ
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);


        if (Physics.Raycast(ray, out hit, 1000))
        {
            //������ �������� ��� ��ġ �ľ�
            targetPoint = hit.point;
        }
        else
        {   //10�� ���� ��Ÿ��� ����
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        //������ �÷��̾ �ٶ󺸴� �������� ����
        Vector3 attackDirection = Head.transform.forward;

        // �Ѿ��� ��ġ�� ���� ������ ����
        bullet.transform.position = gunEndPos;

        // �Ѿ��� ���� ����
        bullet.transform.rotation = Quaternion.LookRotation(attackDirection);

        // �Ѿ˿� ���� ����
        bullet.GetComponent<Rigidbody>().velocity = attackDirection * attackSpd;

        if (Physics.Raycast(gunEndPos, attackDirection, out hit, 1000))
        {

        }

    }
}
