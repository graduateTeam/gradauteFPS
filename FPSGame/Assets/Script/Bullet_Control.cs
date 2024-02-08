using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //�̱��� ������� ������Ʈ ����
    public static GameManager gm_instance;

    public Vector3 gunEndPos;

    [Header("WeaponAssault")]
    [SerializeField]
    public WeaponAssaultRifle weapon;
    // Start is called before the first frame update

    private float attackDis;
    private float attackSpd;

    public GameObject player;
    public Rigidbody rb_player;
    public Collider gunCollider;

    public void getFromPC(GameObject player, MeshCollider mesh_gun)
    {
        this.player = player;

        this.rb_player = player.GetComponent<Rigidbody>();
        this.gunCollider = rb_player.GetComponent<Collider>();

        GameObject gun = null;

        Transform gunTransform = player.transform.Find("Weapon");
        if (gunTransform != null)
        {
            gun = gunTransform.gameObject;
        }

        this.gunEndPos = mesh_gun.transform.position + (rb_player.transform.forward *0.75f);
        this.gunEndPos += rb_player.transform.right * gun.transform.localPosition.x;
        this.gunEndPos.y -= 0.3f;   //�ѱ� ������ �߻�Ǵ� �� ���� ���̰� y��ǥ ������ �ϰ�

    }

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
        if (collision.gameObject.tag != "bullet" && collision.gameObject.name != "Weapon")    //�Ѿ˳��� �ε����� ������� ��츦 ����
        {
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
        Ray ray = new Ray(rb_player.transform.position, rb_player.transform.rotation * oldray.direction);


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
        Vector3 attackDirection = rb_player.transform.forward;

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
