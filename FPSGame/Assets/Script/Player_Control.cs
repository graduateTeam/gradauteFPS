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

/*리스폰 코루틴 부분 살짝 불안 나중에 주의깊게 쳐다볼 것*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public float speed; //플레이어의 속도
    public bool alive; //플레이어의 생존 여부 이게 꺼져있으면 플레이어의 조작권한을 뺏어오고, 리스폰 후에 다시 부여해줄 것임
    public bool isJump; //무한 점프 방지용 점프를 하고 난 뒤 땅에 착지해야 다시 false로 만들어줄거임
    public bool wasd; //점프 시 wasd 막기
    public float jumpPower; //점프력
    public float lim_Speed; //최대 속력
    public float Respawn_Time;    //리스폰 시간

    [SyncVar]
    public int HP;  //플레이어의 체력

    public GameObject Attack_point; //혹시 피격을 위한 오브젝트
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

    private void OnCollisionEnter(Collision collision)
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

    public void Hitted_Bullet(int damage)   //CmdReduceHP의 외부접근을 위한 함수
    {
        CmdReduceHP(damage);
    }

    [Command]
    void CmdReduceHP(int damage)
    {
        HP -= damage;
    }

    [Command]   //Command가 있으면 클라이언트에서 호출하지만 처리는 서버(Mirror)에서 함
    void CmdFire()  //총알 발사 서버에서의 처리
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
        //죽는 소리 및 애니메이션
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

    [ClientRpc] //모든 클라이언트에서 서버의 명령을 받고 해당 플레이어를 부활로 처리
    void RpcRespawn()
    {
        //부활 및 좌표 변경
    }
}
