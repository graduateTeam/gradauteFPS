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
/*������ ó���� ���ؼ��� Network Identity�� �ʼ������� �ν����� â���� �Ҵ��� ����Ѵ�*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public bool alive; //�÷��̾��� ���� ���� �̰� ���������� �÷��̾��� ���۱����� �������, ������ �Ŀ� �ٽ� �ο����� ����
    public bool isJump; //���� ���� ������ ������ �ϰ� �� �� ���� �����ؾ� �ٽ� false�� ������ٰ���
    public bool wasd; //���� �� wasd ����

    public float MouseX;
    public float MouseY;    //���콺�� ���� ���� ������

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

    // Update is called once per frame
    void Update()
    {
        player_movement();

        if (HP <= 0)
        {
            CmdplayerDies();
        }

        // if (alive) Rotate();
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
                Debug.Log("������ ��"+vec+" // wasd: "+wasd + " // isJump: " + isJump);
                // �Է��� ������ ����
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdMoveOnServer(vec);
                }

            }

            if (!isJump && Input.GetButtonDown("Jump"))
            {
                isJump = true; //2�� ���� ����
                wasd = false;   //���� �� ������ ����

                vec = new Vector3(0, jumpPower, 0);
                Debug.Log("��������"+vec+" // wasd: "+wasd+" // isJump: "+isJump);
                // �Է��� ������ ����
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdJumpOnServer(vec);
                }
            }

            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("�Ѿ� �߻�");
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdFire();
                }
                
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //�� Ȥ�� ���������� �ٽ� ������ ���� �ٽ� Ȱ��ȭ
        {
            if (collision.gameObject.tag == "land")
            {
                Debug.Log("���� ������");
                isJump = false;
                wasd = true;
            }
        }
    }

    public override void OnStartServer()
    {
        CmdSpawnPlayers();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer && !NetworkClient.ready)
        {
            NetworkClient.Ready();
        }
    }

    //������ ���ۿ� �����Ӱ� ü�°��Ҹ� �ϱ� ���� �ڵ�
    [Command]
    void CmdMoveOnServer(Vector3 move)
    {
        rb_player.AddForce(move, ForceMode.Impulse);
        rb_player.velocity = Vector3.ClampMagnitude(rb_player.velocity, lim_Speed);

        rb_weapon.AddForce(rb_player.velocity, ForceMode.Impulse);
    }

    [Command]
    void CmdJumpOnServer(Vector3 jump)
    {
        rb_player.AddForce(jump, ForceMode.VelocityChange);

        rb_weapon.AddForce(rb_player.velocity, ForceMode.VelocityChange);
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
        GameObject bullet = Bullet_Pool.instance.GetBullet();   //bullet�� Null�� �� �ϴ� ���ĵΰ�

        if (bullet != null)
        {
            //�Ѿ��� ��ġ �� ���� ����
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;

            bullet.transform.localPosition = rb_weapon.transform.forward;
            bullet.transform.localRotation = rb_weapon.rotation;

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
        Debug.Log("���� �׾��ٰ� ���°ų�?");
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

    private void Rotate()   //�̷��� �ϰԵǸ� ���콺�� ���� �÷��̾ ������ Ʋ����, ���� �� �ڿ������� �������� ���� �Ӹ� ������ ������ �������� �� ��?
    {
        MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

        MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

        MouseY = Mathf.Clamp(MouseY, -75f, 75f);    //�� �Ʒ� �� �ִ� ���� -75 ~ 75

        transform.localRotation = Quaternion.Euler(MouseY, -MouseX, 0f);
    }//ť������� ������Ʈ�� ������ ������ �־ �ӽ� ����

    [Command]
    private void CmdSpawnPlayers()  //���������� ���� �÷��̾� ��ġ���� �Ҵ�Ǿ�� �Ѵ�.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(0, 0, 0);

        NetworkServer.Spawn(Attack_point); //�� ���� ���� ����Ʈ(�θ�)�� ��ȯ�ؾ� �н��� �� �����.

        RpcSetPlayer(Attack_point);
    }

    [ClientRpc]
    private void RpcSetPlayer(GameObject Attack_Object)
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

        alive = true;
        isJump = false;
        wasd = true;

        HP = 100;
    }

}
