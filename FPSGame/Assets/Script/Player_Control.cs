using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
/*
* 게임 방에 들어갈 플레이어의 프리팹으로
* 이 곳에 플레이어의 움직임, HP등을 관리한다면
* 서버를 통해 게임을 진행시키는 것과 같고
* 클라이언트에서 그런 중요정보를 처리하지 않음으로써
* 메이플의 클라이언트 변조사건 등을 막을 수 있다.
*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public float speed; //플레이어의 속도
    public bool alive; //플레이어의 생존 여부 이게 꺼져있으면 플레이어의 조작권한을 뺏어오고, 리스폰 후에 다시 부여해줄 것임
    public bool isJump; //무한 점프 방지용 점프를 하고 난 뒤 땅에 착지해야 다시 false로 만들어줄거임
    public bool wasd; //점프 시 wasd 막기
    public float jumpPower; //점프력
    public float lim_Speed; //최대 속력

    [SyncVar]
    public int HP;  //플레이어의 체력

    public Rigidbody player;   //플레이어 몸뚱아리
    public Rigidbody weapon;   //무기

    void Start()
    {
        getStart();
    }

    // Update is called once per frame
    void Update()
    {
        player_movement();

    }

    private void getStart()
    {
        alive = true;
        isJump = false;
        wasd = true;

        HP = 100;

        player = GetComponent<Rigidbody>();
        weapon = GetComponent<Rigidbody>();
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
                // 입력을 서버에 전송
                CmdMoveOnServer(vec);

            }

            if (!isJump && Input.GetButtonDown("Jump"))
            {
                isJump = true; //2중 점프 막기
                wasd = false;   //점프 중 움직임 막기

                vec = new Vector3(player.velocity.x, jumpPower, player.velocity.z);
                // 입력을 서버에 전송
                CmdMoveOnServer(vec);

            }

            if (Input.GetButtonDown("Fire1"))
            {
                CmdFire();
            }

        }
    }

    private void OnCollisionStart(Collision collision)
    {
        if (alive)   //땅 혹은 지형지물에 다시 닿으면 점프 다시 활성화
        {
            if (collision.gameObject.tag == "land")
            {
                isJump = false;
                wasd = true;
            }
        }
    }

    //서버에 전송에 움직임과 체력감소를 하기 위한 코드
    [Command]
    void CmdMoveOnServer(Vector3 move)
    {
        if (player.velocity.magnitude < lim_Speed) player.velocity = move;
    }

    [Command]
    void CmdReduceHP(int damage)
    {
        HP -= damage;
    }

    [Command]
    void CmdFire()
    {
        GameObject bullet = Bullet_Pool.instance.GetBullet();

        if (bullet != null)
        {
            //총알의 위치 및 방향 설정
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;

            bullet.transform.localPosition = weapon.transform.forward;
            bullet.transform.localRotation = weapon.rotation;

            //총알을 네트워크에서 생성
            NetworkServer.Spawn(bullet);

            //총알 속도
            Vector3 b_vec = bullet.transform.localPosition * 350;  //임의로 350으로
            CmdBulletMoveOnServer(b_vec, bullet);
        }
    }

    [Command]
    void CmdBulletMoveOnServer(Vector3 b_move, GameObject bullet)
    {
        bullet.GetComponent<Rigidbody>().velocity = b_move;
    }
}
