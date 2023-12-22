using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.UIElements;
/*
* ���� �濡 �� �÷��̾��� ����������
* �� ���� �÷��̾��� ������, HP���� �����Ѵٸ�
* ������ ���� ������ �����Ű�� �Ͱ� ����
* Ŭ���̾�Ʈ���� �׷� �߿������� ó������ �������ν�
* �������� Ŭ���̾�Ʈ ������� ���� ���� �� �ִ�.
*/

/*������ �ڷ�ƾ �κ� ��¦ �Ҿ� ���߿� ���Ǳ�� �Ĵٺ� ��*/
/*������ ó���� ���ؼ��� Network Identity�� �ʼ������� �ν����� â���� �Ҵ��� ����Ѵ�*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public bool alive; //�÷��̾��� ���� ���� �̰� ���������� �÷��̾��� ���۱����� �������, ������ �Ŀ� �ٽ� �ο����� ����
    public bool isJump; //���� ���� ������ ������ �ϰ� �� �� ���� �����ؾ� �ٽ� false�� ������ٰ���
    public bool wasd; //���� �� wasd ����

    public float MouseX;
    public float MouseY;    //���콺�� ���� ���� ������
    private Vector3 jumpDirection;

    [SyncVar]
    public int HP;  //�÷��̾��� ü��

    [SyncVar]
    public float lim_Speed; //�ִ� �ӷ�

    [SyncVar]
    public float jumpPower; //������

    [SyncVar]
    public float speed; //�÷��̾��� �ӵ�

    [SyncVar]
    public float MouseSen;

    [SyncVar]
    public float Respawn_Time;    //������ �ð�

    public GameObject Attack_point; //Ȥ�� �ǰ��� ���� ������Ʈ
    
    public Rigidbody rb_player; //�÷��̾��� ������ٵ�

    public Rigidbody rb_weapon; //������ ������ٵ�

    public Bullet_Control bc;
    public GameManager gm;

    private float recoilAmount = 1.0f; // �ݵ��� ��
    private float recoilRecoverySpeed = 3.0f; // �ݵ� ȸ�� �ӵ�
    private float currentRecoil = 0.0f; // ���� �ݵ� ���� //Weapon_info���� ���� �޾ƿ� ����

    [SyncVar]
    public bool canFire = true;

    // Update is called once per frame
    void FixedUpdate()
    {
        player_movement();

        if (HP <= 0)
        {
            CmdplayerDies();
        }

        Rotate();
    }

    private void Update()
    {
        gm.HP_UI_Update(HP);

        // �ݵ��� �ε巴�� �����Ѵ�.
        if (currentRecoil > 0 && !canFire)  //�ð��� ������ = �� �߻� ��(canFire == currentRecoil�� ����� �������� ��)
        {
            MouseY -= currentRecoil * Time.deltaTime;   //�ð��� ���� �ݵ�����(!canFire�� �ð��� ����� ���� ���� ���ϰ� �и�)
            currentRecoil -= recoilRecoverySpeed * Time.deltaTime;  //�ð��� ���� �ݵ�ȸ��(�׸�ŭ ȸ���� ���� ������)
            currentRecoil = Mathf.Max(currentRecoil, 0);
        }
    }

    private void player_movement()
    {
        if (!isLocalPlayer) return;

        Vector3 vec;

        if (alive)
        {
            if (wasd)
            {
                float Horizontal_move = Input.GetAxis("Horizontal");
                float Vertical_move = Input.GetAxis("Vertical");

                jumpDirection = new Vector3(Horizontal_move, 0, Vertical_move);

                vec = transform.TransformDirection(jumpDirection) * speed * Time.deltaTime;

                jumpDirection = vec;
                if (isOwned)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                    rb_player.velocity = Vector3.ClampMagnitude(rb_player.velocity, lim_Speed);
                }

            }

            if (!isJump && Input.GetButton("Jump"))
            {
                isJump = true; //2�� ���� ����
                wasd = false;   //���� �� ������ ����

                vec = transform.TransformDirection(new Vector3(jumpDirection.x, jumpPower, jumpDirection.z)) * speed * Time.deltaTime;
                Debug.Log("����");
                if (isOwned)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                }
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

    [ClientRpc]
    public void Hitted_Bullet(int damage)   //CmdReduceHP�� �ܺ������� ���� �Լ�
    {
        CmdReduceHP(damage);
    }

    [Command]
    void CmdReduceHP(int damage)
    {
        HP -= damage;
    }

    [Command]   //Command�� ������ Ŭ���̾�Ʈ���� ȣ�������� ó���� ����(Mirror)���� ��
    void CmdFire()  //�Ѿ� �߻� ���������� ó��
    {
        try
        {
            StartCoroutine("Weapon_delay");
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    void CmdplayerDies()
    {
        alive = false;
        HP = 0;
        RpcplayerDies();
        StartCoroutine("Respawn");
    }

    [ClientRpc]
    void RpcplayerDies()
    {
        //�״� �Ҹ� �� �ִϸ��̼�
    }

    IEnumerator Respawn()
    {
        float startTime = Time.time;
        float elapsedTime = 0;

        while (elapsedTime < Respawn_Time)
        {
            elapsedTime = Time.time - startTime;
            gm.Respawn_bar_Update(elapsedTime, Respawn_Time);

            yield return null;  // ���� �����ӱ��� ����մϴ�.
        }

        if (isServer)
        {
            HP = 100;
            alive = true;
            RpcRespawn();
        }
    }

    IEnumerator Weapon_delay()
    {
        canFire = false;
        currentRecoil += recoilAmount;  //�ݵ��� ���ذ���.
        bc.Bullet_Shoot(rb_weapon);

        yield return new WaitForSeconds(0.2f);  //���ǰ�
        canFire = true;
    }

    [ClientRpc] //��� Ŭ���̾�Ʈ���� ������ ����� �ް� �ش� �÷��̾ ��Ȱ�� ó��
    void RpcRespawn()
    {
        //��Ȱ �� ��ǥ ����
    }

    private void Rotate()   //�̷��� �ϰԵǸ� ���콺�� ���� �÷��̾ ������ Ʋ����, ���� �� �ڿ������� �������� ���� �Ӹ� ������ ������ �������� �� ��?
    {
        if(alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -90f, 90f);    //�� �Ʒ� �� �ִ� ���� -75 ~ 75

            Quaternion quat = Quaternion.Euler(new Vector3(MouseY, -MouseX, 0));
            transform.rotation
                = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime *MouseSen);
        }
    }

    [Server]
    private void SpawnPlayer()  //���������� ���� �÷��̾� ��ġ���� �Ҵ�Ǿ�� �Ѵ�.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(0, 0, 0);
        Attack_point.transform.localRotation = Quaternion.Euler(0, 0, 0);

        NetworkServer.Spawn(Attack_point); //�� ���� ���� ����Ʈ(�θ�)�� ��ȯ�ؾ� �н��� �� �����.

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

        bc = Bullet_Control.bc_instance;    //NetworkBehavior�� ��ӹް� �ȴٸ� ��� new ~~ �̷� ���� �ν��Ͻ�ȭ�� �Ұ����ϰ� �ȴ�.

        gm = GameManager.gm_instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //�� Ȥ�� ���������� �ٽ� ������ ���� �ٽ� Ȱ��ȭ
        {
            if (collision.gameObject.tag == "land")
            {
                isJump = false;
                wasd = true;

                //rb_player.velocity = new Vector3(jumpDirection.x, 0, jumpDirection.z);
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

            Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //���Ƿ� ����� 10�̶�� �� �� ��ũ��Ʈ�� ������� �������� �� ������� �ٲ� ��
        }
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //������ ��� ����� ���� �߽����κ��� �Ÿ��� ���� ����� �氨�� ���� ������
    }
}
