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
/*서버적 처리를 위해서는 Network Identity를 필수적으로 인스펙터 창에서 할당해 줘야한다*/
public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public bool alive; //플레이어의 생존 여부 이게 꺼져있으면 플레이어의 조작권한을 뺏어오고, 리스폰 후에 다시 부여해줄 것임
    public bool isJump; //무한 점프 방지용 점프를 하고 난 뒤 땅에 착지해야 다시 false로 만들어줄거임
    public bool wasd; //점프 시 wasd 막기

    public float MouseX;
    public float MouseY;    //마우스에 따른 몸의 움직임

    [SyncVar]
    public int HP;  //플레이어의 체력

    [SyncVar]
    public float lim_Speed; //최대 속력

    [SyncVar]
    public float jumpPower; //점프력

    [SyncVar]
    public float speed; //플레이어의 속도

    [SyncVar]
    public float MouseSen;

    [SyncVar]
    public float Respawn_Time;    //리스폰 시간

    public GameObject Attack_point; //혹시 피격을 위한 오브젝트
    
    public Rigidbody rb_player; //플레이어의 리지드바디

    public Rigidbody rb_weapon; //무기의 리지드바디

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
                Debug.Log("움직임 중"+vec+" // wasd: "+wasd + " // isJump: " + isJump);
                // 입력을 서버에 전송
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdMoveOnServer(vec);
                }

            }

            if (!isJump && Input.GetButtonDown("Jump"))
            {
                isJump = true; //2중 점프 막기
                wasd = false;   //점프 중 움직임 막기

                vec = new Vector3(0, jumpPower, 0);
                Debug.Log("점프점프"+vec+" // wasd: "+wasd+" // isJump: "+isJump);
                // 입력을 서버에 전송
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdJumpOnServer(vec);
                }
            }

            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("총알 발사");
                if (isLocalPlayer && NetworkClient.ready)
                {
                    CmdFire();
                }
                
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //땅 혹은 지형지물에 다시 닿으면 점프 다시 활성화
        {
            if (collision.gameObject.tag == "land")
            {
                Debug.Log("땅에 착지함");
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

    //서버에 전송에 움직임과 체력감소를 하기 위한 코드
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
        GameObject bullet = Bullet_Pool.instance.GetBullet();   //bullet이 Null인 건 일단 제쳐두고

        if (bullet != null)
        {
            //총알의 위치 및 방향 설정
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;

            bullet.transform.localPosition = rb_weapon.transform.forward;
            bullet.transform.localRotation = rb_weapon.rotation;

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
        Debug.Log("지금 죽었다고 보는거냐?");
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

    private void Rotate()   //이렇게 하게되면 마우스에 따라 플레이어가 각도를 틀지만, 조금 더 자연스러운 움직임을 위해 머리 몸통을 나눠서 움직여야 할 듯?
    {
        MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

        MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

        MouseY = Mathf.Clamp(MouseY, -75f, 75f);    //위 아래 고개 최대 범위 -75 ~ 75

        transform.localRotation = Quaternion.Euler(MouseY, -MouseX, 0f);
    }//큐브모형의 오브젝트가 구르는 문제가 있어서 임시 폐지

    [Command]
    private void CmdSpawnPlayers()  //서버적으로 각종 플레이어 수치들이 할당되어야 한다.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(0, 0, 0);

        NetworkServer.Spawn(Attack_point); //맨 위의 어택 포인트(부모)만 소환해야 분신이 안 생긴다.

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
