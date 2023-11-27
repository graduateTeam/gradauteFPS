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

    public Bullet_Pool bullet_Class;

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

                vec = transform.TransformDirection(new Vector3(Horizontal_move, 0, Vertical_move)) * speed * Time.deltaTime;
                Debug.Log("������");
                if (hasAuthority)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                    rb_player.velocity = Vector3.ClampMagnitude(rb_player.velocity, lim_Speed);
                }

            }

            if (!isJump && Input.GetButtonDown("Jump"))
            {
                isJump = true; //2�� ���� ����
                wasd = false;   //���� �� ������ ����

                vec = transform.TransformDirection(new Vector3(0, jumpPower, 0)) * speed * Time.deltaTime;
                Debug.Log("����");
                if (hasAuthority)
                {
                    rb_player.AddForce(vec, ForceMode.Impulse);
                }
            }

            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("�Ѿ� �߻�");
                if (isLocalPlayer && NetworkClient.ready)
                {
                    //CmdFire();
                }
                
            }
            Debug.Log("wasd: " + wasd + " // isJump: " + isJump);
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
        SpawnPlayer();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer && !NetworkClient.ready)
        {
            NetworkClient.Ready();
        }
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
            bullet_Class.MoveBullet(b_vec);
            
        }
        else
        {
            Debug.Log("�Ѿ� �غ� �� ��");
        }
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
        if(alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -90f, 90f);    //�� �Ʒ� �� �ִ� ���� -75 ~ 75

            Quaternion quat = Quaternion.Euler(new Vector3(MouseY, -MouseX, 0));
            transform.rotation
                = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime *MouseSen);
        }
    }//ť������� ������Ʈ�� ������ ������ �־ �ӽ� ����

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

        bullet_Class = new Bullet_Pool();
    }
}
