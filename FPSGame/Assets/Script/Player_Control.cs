using System.Collections;
using UnityEngine;
using Mirror;
using System;

/*
* ���� GameManabger�� �и� ��ų ������������ ��ġ ��������.
*/

public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public bool alive; //�÷��̾��� ���� ���� �̰� ���������� �÷��̾��� ���۱����� �������? ������ �Ŀ� �ٽ� �ο����� ����
    public bool isJump; //���� ���� ������ ������ �ϰ� �� �� ���� �����ؾ� �ٽ� false�� ������ٰ���?
    public bool wasd; //���� �� wasd ����

    public float MouseX;
    public float MouseY;    //���콺�� ���� ���� ������
    private Vector3 jumpDirection;

    //[SyncVar]
    public bool moving;

    //[SyncVar]
    public int HP;  //�÷��̾��� ü��

    //[SyncVar]
    public float lim_Speed; //�ִ� �ӷ�

    //[SyncVar]
    public float jumpPower; //������

    //[SyncVar]
    public float speed; //�÷��̾��� �ӵ�

    //[SyncVar]
    public float MouseSen;

    //[SyncVar]
    public float Respawn_Time;    //������ �ð�

    //[SyncVar]
    public float attackRate;    //�� ���� ����

    //[SyncVar]
    public GameObject Head;

    //[SyncVar]
    public GameObject Arm;

    public WeaponAssaultRifle AssaultRifle;

    public float currentRecoil; // ���� �ݵ� ����

    public float recoilAmount;  // �ݵ��� ��
    public float origin_recoilAmount;

    public float recoilRecoverySpeed;   // �ݵ� ȸ�� �ӵ�
    public float origin_recoilRecoverySpeed;

    public GameObject Attack_point; //Ȥ�� �ǰ��� ���� ������Ʈ

    public Rigidbody rb_player; //�÷��̾��� ������ٵ�?

    public Rigidbody rb_weapon; //������ ������ٵ�?

    public GameObject Feet;

    public Bullet_Control bc;
    public GameManager gm;

    //[SyncVar]
    public bool canFire = true;

    // Update is called once per frame
    private void Start()
    {
        AssaultRifle = this.GetComponentInChildren<WeaponAssaultRifle>();

        attackRate = AssaultRifle.weapon_attackRate();
    }
    void FixedUpdate()
    {
        player_movement();

        if (HP <= 0)
        {
            CmdplayerDies();
        }

        Rotate();

        Rebound();

        RaycastHit hit;
        // �� ��ġ���� �������� Ray�� ���?
        if (Physics.Raycast(Feet.transform.position, Feet.transform.forward, out hit, 1f))
        {
            // ��ֹ���?���� üũ
            if (hit.transform.position.y >= (Feet.transform.position.y + 2) && hit.transform.position.y < (Feet.transform.position.y + 1)) // '2'�� ���� �� �ִ� �ִ� ����
            {
                //��ֹ�?�ѱ�
                Vector3 jumpForce = new Vector3(0, CalculateJumpVerticalSpeed(), 0);
                rb_player.AddForce(jumpForce, ForceMode.Impulse);
            }
        }

    }

    private void Update()
    {
        float[] reciveFromWeapon = AssaultRifle.giveToPC();

        /*���?������?���� �����ϱ� ���� last�� ���� �Ѵ�
        ������ �� �� 0 �� ��, ���� �޾ƿ��� �ʴ� ������ �ֱ� ������ 0�� ���� ������ �޾ƿ����� �Ѵ�.*/

        if (recoilAmount != reciveFromWeapon[0] || (recoilAmount == 0 && reciveFromWeapon[0] == recoilAmount))
        {
            recoilAmount = reciveFromWeapon[0];
            origin_recoilAmount = recoilAmount;
        }

        if (recoilRecoverySpeed != reciveFromWeapon[1] || (recoilRecoverySpeed == 0 && reciveFromWeapon[1] == recoilRecoverySpeed))
        {
            recoilRecoverySpeed = reciveFromWeapon[1];
            origin_recoilRecoverySpeed = recoilRecoverySpeed;
        }

        gm.HP_UI_Update(HP);

        if (NetworkServer.active && !gm.Time_isMinus())
        {
            //Time_spent();
        }

        bc.getFromPC(Attack_point);
    }

    public void Rebound()   //�ݵ��Լ�

    {
        // �ݵ��� �ε巴�� �����Ѵ�.
        if (AssaultRifle.canShoot && !canFire)  //�ð��� ������
        {
            MouseY += currentRecoil * Time.deltaTime; // �ð��� ���� �ݵ� ����
            currentRecoil -= recoilRecoverySpeed * Time.deltaTime; // �ð��� ���� �ݵ� ����
            currentRecoil = Mathf.Max(currentRecoil, recoilAmount * 0.9f); // �ݵ��� 0���� �۾����� �ʵ��� ��
        }
        else // ���� ���� �ʴ� ���¶��?
        {
            if (currentRecoil != origin_recoilAmount)
                currentRecoil = origin_recoilAmount; // �ݵ��� ������ ������ ����

            if (recoilRecoverySpeed != origin_recoilRecoverySpeed)
                recoilRecoverySpeed = origin_recoilRecoverySpeed;
        }
    }

    // ���� �ӵ� ���?
    float CalculateJumpVerticalSpeed()
    {
        // ���������� ���� ���̸� ����մϴ�?(���� = 0.5 * �߷� * �����ð�^2)
        return Mathf.Sqrt(2 * 2 * Physics.gravity.magnitude);
    }

    private void player_movement()  //�÷��̾� ������ �Լ�
    {
        if (!isLocalPlayer) return;

        Vector3 vec;

        if (alive)
        {
            if (wasd)
            {
                float Horizontal_move = Input.GetAxis("Horizontal");
                float Vertical_move = Input.GetAxis("Vertical");

                /* �̵� ���� ���?
                   - transform.right * Horizontal_move: ���� �Է¿� ���� �̵� ����
                   - new Vector3(transform.forward.x, 0, transform.forward.z) * Vertical_move: ���� �Է¿� ���� �̵� ����, y ���� 0���� �����Ͽ� ���������θ� ������
                   - �� �̵� ���� ���� �� .normalized�� ũ�� 1�� ���� -> �밢�� �̵� �� �ӵ� �����ϰ� ���� */
                Vector3 moveDirection = (transform.right * Horizontal_move + new Vector3(Head.transform.forward.x, 0, Head.transform.forward.z) * Vertical_move).normalized;



                // ���� �̵� ���� ���?moveDirection�� �̵� �ӵ�(speed)�� ������ �� �ð�(Time.deltaTime) ����->������ �ӵ��� ������
                vec = moveDirection * speed * Time.deltaTime;

                if (!moving)
                {
                    moving = true;
                    recoilAmount *= 1.5f;
                    recoilRecoverySpeed *= 0.8f;
                }
                if (isOwned)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                    rb_player.velocity = Vector3.ClampMagnitude(rb_player.velocity, lim_Speed);
                    jumpDirection = Vector3.zero;
                }

            }
            else
            {
                moving = false;
            }

            if (!isJump && Input.GetButton("Jump"))
            {
                isJump = true; //2�� ���� ����
                wasd = false;   //���� �� ������ ����

                // ���� ��ǥ�� �������� ���� ���� ����
                vec = new Vector3(jumpDirection.x, jumpPower, jumpDirection.z) * speed * Time.deltaTime;

                if (!moving)
                {
                    moving = true;
                    recoilAmount *= 1.5f;
                    recoilRecoverySpeed *= 0.8f;
                }

                if (isOwned)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                }
            }
            else
            {
                moving = false;
            }

            if (Input.GetButton("Fire1"))
            {
                if (isLocalPlayer && NetworkClient.ready && canFire)
                {
                    CmdFire();
                }
            }
        }
    }

    public override void OnStartServer()
    {
        SpawnPlayer();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer && !NetworkClient.ready)
        {
            NetworkClient.Ready();
        }
    }

    [Command]
    public void Hitted_Bullet(int damage)   //CmdReduceHP�� �ܺ������� ���� �Լ� �� ��� �Լ�
    {
        CmdReduceHP(damage);
    }

    //[Command]
    void CmdReduceHP(int damage)
    {
        if (HP > damage)
            HP -= damage;
        else
            HP = 0;
    }

    //[Command]   //Command�� ������ Ŭ���̾�Ʈ���� ȣ�������� ó���� ����(Mirror)���� ��
    void CmdFire()  //�Ѿ� �߻� ���������� ó��
    {
        try
        {
            StartCoroutine("Weapon_delay");
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    //[Command]
    void CmdplayerDies()
    {
        alive = false;
        HP = 0;
        RpcplayerDies();
        //StartCoroutine("Respawn");
    }

    //[ClientRpc]
    void RpcplayerDies()
    {
        //�״� �Ҹ� �� �ִϸ��̼�
    }

    /*IEnumerator Respawn()
    {
        float startTime = Time.time;
        float elapsedTime = 0;

        while (elapsedTime < Respawn_Time)
        {
            elapsedTime = Time.time - startTime;
            gm.Respawn_bar_Update(elapsedTime, Respawn_Time);

            yield return null;  // ���� �����ӱ��� ����մϴ�?
        }

        if (isServer)
        {
            HP = 100;
            alive = true;
            RpcRespawn();
        }
    }*/

    IEnumerator Weapon_delay()
    {
        canFire = false;
        try
        {
            bc.Bullet_Shoot();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        yield return new WaitForSeconds(attackRate);  //���ǰ�
        canFire = true;
    }

    /*[ClientRpc] //���?Ŭ���̾�Ʈ���� ������ ������ �ް� �ش� �÷��̾ ��Ȱ�� ó��
    void RpcRespawn()
    {
        //��Ȱ �� ��ǥ ����
    }
*/
    private void Rotate()   //�̷��� �ϰԵǸ� ���콺�� ���� �÷��̾ ������ Ʋ����, ���� �� �ڿ������� �������� ���� �Ӹ� ������ ������ �������� �� ��?
    {
        if (alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -70f, 70f);    //�� �Ʒ� ���� �ִ� ���� -70 ~ 70
        }

        Quaternion quat = Quaternion.Euler(new Vector3(-MouseY, MouseX, 0));

        Body_Rotate(quat, Head);
        Body_Rotate(quat, Arm);
    }
    private void Body_Rotate(Quaternion quat, GameObject g_object)
    {
        g_object.transform.rotation
            = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime * MouseSen);
    }

    //[Server]
    private void SpawnPlayer()  //���������� ���� �÷��̾� ��ġ���� �Ҵ�Ǿ�� �Ѵ�.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(55f, 0, -0.6140758f);
        Attack_point.transform.localRotation = Quaternion.Euler(0, 0, 0);

        NetworkServer.Spawn(Attack_point); //�� ���� ���� ����Ʈ(�θ�)�� ��ȯ�ؾ� �н��� �� �����?

        Game_Start(Attack_point);
    }

    private void Game_Start(GameObject Attack_Object)
    {
        rb_player = Attack_Object.GetComponent<Rigidbody>();
        rb_weapon = Attack_Object.GetComponent<Rigidbody>();

        if (rb_player == null)
        {
            Debug.Log("rb_player is null");
        }

        if (rb_weapon == null)
        {
            Debug.Log("rb_weapon is null");
        }

        bc = Bullet_Control.bc_instance;    //NetworkBehavior�� ��ӹް�?�ȴٸ� ���?new ~~ �̷� ���� �ν��Ͻ�ȭ�� �Ұ����ϰ� �ȴ�.

        bc.weapon = Attack_Object.GetComponentInChildren<WeaponAssaultRifle>();

        gm = GameManager.gm_instance;

        gm.UI_Init();   //UI �ʱ�ȭ

        moving = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //�� Ȥ�� ���������� �ٽ� ������ ���� �ٽ� Ȱ��ȭ
        {
            if (collision.gameObject.tag == "land")
            {
                isJump = false;
                wasd = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "bullet")
        {
            Debug.Log("�÷��̾��� �ǰ�");
            Vector3 player_pos = transform.position;  //�÷��̾ �� �ִ� ��ġ
            Vector3 bullet_pos = other.transform.position;

            Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //���Ƿ� �����?10�̶��?�� �� ��ũ��Ʈ�� �������?�������� �� �������?�ٲ� ��
        }
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //������ ���?�����?���� �߽����κ��� �Ÿ��� ���� �����?�氨�� ����?������
    }
    /*
    //[Server]
    private void Time_spent()   //�ð� �帣�� �� ����
    {
        float[] t_res = gm.Time_go();
        Time_Update(t_res[0], t_res[1]);
    }

    //[ClientRpc]
    public void Time_Update(float m, float s)   //�ð� UI Update
    {
        string sec = s < 10 ? "0" + s.ToString() : s.ToString();
        string min = m.ToString() + ":";

        gm.game_Time_UI.text = string.Format(min + sec);
    }
    */
}