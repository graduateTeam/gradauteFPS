using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.UIElements;
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
    private Vector3 jumpDirection;

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

    public Bullet_Control bc;
    public GameManager gm;

    private float recoilAmount = 1.0f; // 반동의 양
    private float recoilRecoverySpeed = 3.0f; // 반동 회복 속도
    private float currentRecoil = 0.0f; // 현재 반동 상태 //Weapon_info에서 추후 받아올 것임

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

        // 반동을 부드럽게 적용한다.
        if (currentRecoil > 0 && !canFire)  //시간에 따른다 = 총 발사 후(canFire == currentRecoil이 양수로 더해지는 것)
        {
            MouseY -= currentRecoil * Time.deltaTime;   //시간에 따른 반동적용(!canFire인 시간이 길어질 수록 위로 심하게 밀림)
            currentRecoil -= recoilRecoverySpeed * Time.deltaTime;  //시간에 따른 반동회복(그만큼 회복도 많이 시켜줌)
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
                isJump = true; //2중 점프 막기
                wasd = false;   //점프 중 움직임 막기

                vec = transform.TransformDirection(new Vector3(jumpDirection.x, jumpPower, jumpDirection.z)) * speed * Time.deltaTime;
                Debug.Log("점프");
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
        //죽는 소리 및 애니메이션
    }

    IEnumerator Respawn()
    {
        float startTime = Time.time;
        float elapsedTime = 0;

        while (elapsedTime < Respawn_Time)
        {
            elapsedTime = Time.time - startTime;
            gm.Respawn_bar_Update(elapsedTime, Respawn_Time);

            yield return null;  // 다음 프레임까지 대기합니다.
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
        currentRecoil += recoilAmount;  //반동을 더해간다.
        bc.Bullet_Shoot(rb_weapon);

        yield return new WaitForSeconds(0.2f);  //임의값
        canFire = true;
    }

    [ClientRpc] //모든 클라이언트에서 서버의 명령을 받고 해당 플레이어를 부활로 처리
    void RpcRespawn()
    {
        //부활 및 좌표 변경
    }

    private void Rotate()   //이렇게 하게되면 마우스에 따라 플레이어가 각도를 틀지만, 조금 더 자연스러운 움직임을 위해 머리 몸통을 나눠서 움직여야 할 듯?
    {
        if(alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -90f, 90f);    //위 아래 고개 최대 범위 -75 ~ 75

            Quaternion quat = Quaternion.Euler(new Vector3(MouseY, -MouseX, 0));
            transform.rotation
                = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime *MouseSen);
        }
    }

    [Server]
    private void SpawnPlayer()  //서버적으로 각종 플레이어 수치들이 할당되어야 한다.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(0, 0, 0);
        Attack_point.transform.localRotation = Quaternion.Euler(0, 0, 0);

        NetworkServer.Spawn(Attack_point); //맨 위의 어택 포인트(부모)만 소환해야 분신이 안 생긴다.

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

        bc = Bullet_Control.bc_instance;    //NetworkBehavior를 상속받게 된다면 통상 new ~~ 이런 식의 인스턴스화는 불가능하게 된다.

        gm = GameManager.gm_instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //땅 혹은 지형지물에 다시 닿으면 점프 다시 활성화
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
            Debug.Log("플레이어의 피격");
            Vector3 player_pos = transform.position;  //플레이어가 서 있는 위치
            Vector3 bullet_pos = other.transform.position;

            Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //임의로 대미지 10이라고 한 것 스크립트에 대미지가 정해지면 그 대미지로 바꿀 것
        }
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //임의의 계산 제대로 몸의 중심으로부터 거리에 따른 대미지 경감이 들어갈지 미지수
    }
}
