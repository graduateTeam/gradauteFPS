using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
/*
* ���� �濡 �� �÷��̾��� ����������
* �� ���� �÷��̾��� ������, HP���� �����Ѵٸ�
* ������ ���� ������ �����Ű�� �Ͱ� ����
* Ŭ���̾�Ʈ���� �׷� �߿������� ó������ �������ν�
* �������� Ŭ���̾�Ʈ ������� ���� ���� �� �ִ�.
*/

/*������ �ڷ�ƾ �κ� ��¦ �Ҿ� ���߿� ���Ǳ�� �Ĵٺ� ��*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public float speed; //�÷��̾��� �ӵ�
    public bool alive; //�÷��̾��� ���� ���� �̰� ���������� �÷��̾��� ���۱����� �������, ������ �Ŀ� �ٽ� �ο����� ����
    public bool isJump; //���� ���� ������ ������ �ϰ� �� �� ���� �����ؾ� �ٽ� false�� ������ٰ���
    public bool wasd; //���� �� wasd ����
    public float jumpPower; //������
    public float lim_Speed; //�ִ� �ӷ�
    public float Respawn_Time;    //������ �ð�

    [SyncVar]
    public int HP;  //�÷��̾��� ü��

    public GameObject Attack_point; //Ȥ�� �ǰ��� ���� ������Ʈ
    public Rigidbody player;   //�÷��̾� ���׾Ƹ�
    public Rigidbody weapon;   //����

    void Start()
    {
        getStart();
    }

    // Update is called once per frame
    void Update()
    {
        player_movement();

        if (HP <= 0)
        {
            CmdplayerDies();
        }            
    }

    private void getStart()
    {
        alive = true;
        isJump = false;
        wasd = true;

        HP = 100;

        player = GetComponent<Rigidbody>();
        weapon = GetComponent<Rigidbody>();
        Attack_point = GetComponent<GameObject>();
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

                vec = new Vector3(Horizontal_move, 0, Vertical_move) * speed * Time.deltaTime;
                // �Է��� ������ ����
                CmdMoveOnServer(vec);

            }

            if (!isJump && Input.GetButtonDown("Jump"))
            {
                isJump = true; //2�� ���� ����
                wasd = false;   //���� �� ������ ����

                vec = new Vector3(player.velocity.x, jumpPower, player.velocity.z);
                // �Է��� ������ ����
                CmdMoveOnServer(vec);

            }

            if (Input.GetButtonDown("Fire1"))
            {
                CmdFire();
            }

        }
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

    //������ ���ۿ� �����Ӱ� ü�°��Ҹ� �ϱ� ���� �ڵ�
    [Command]
    void CmdMoveOnServer(Vector3 move)
    {
        if (player.velocity.magnitude < lim_Speed) player.velocity = move;
    }

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
        GameObject bullet = Bullet_Pool.instance.GetBullet();

        if (bullet != null)
        {
            //�Ѿ��� ��ġ �� ���� ����
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;

            bullet.transform.localPosition = weapon.transform.forward;
            bullet.transform.localRotation = weapon.rotation;

            //�Ѿ��� ��Ʈ��ũ���� ����
            NetworkServer.Spawn(bullet);

            //�Ѿ� �ӵ�
            Vector3 b_vec = bullet.transform.localPosition * 350;  //���Ƿ� 350����
            CmdBulletMoveOnServer(b_vec, bullet);
        }
    }

    [Command]
    void CmdBulletMoveOnServer(Vector3 b_move, GameObject bullet)
    {
        bullet.GetComponent<Rigidbody>().velocity = b_move;
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
        yield return new WaitForSeconds(Respawn_Time);

        if(isServer)
        {
            HP = 100;
            alive = true;
            RpcRespawn();
        }
    }

    [ClientRpc] //��� Ŭ���̾�Ʈ���� ������ ����� �ް� �ش� �÷��̾ ��Ȱ�� ó��
    void RpcRespawn()
    {
        //��Ȱ �� ��ǥ ����
    }
}
