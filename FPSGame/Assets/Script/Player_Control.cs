using System.Collections;
using UnityEngine;
using Mirror;
using System;
using UnityEditor.U2D.Sprites;

/*
* 괜히 GameManager로 분리 시킬 생각하지마라 골치 아파진다.
*/

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
    public bool moving;

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

    [SyncVar]
    public float attackRate;    //총 사이 간격

    public WeaponAssaultRifle AssaultRifle;

    public float currentRecoil; // 현재 반동 상태

    public float recoilAmount;  // 반동의 양
    public float origin_recoilAmount;

    public float recoilRecoverySpeed;   // 반동 회복 속도
    public float origin_recoilRecoverySpeed;

    public GameObject Attack_point; //혹시 피격을 위한 오브젝트
    
    public Rigidbody rb_player; //플레이어의 리지드바디

    public Rigidbody rb_weapon; //무기의 리지드바디

    public GameObject Feet;

    public Bullet_Control bc;
    public GameManager gm;

    [SyncVar]
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
        // 발 위치에서 앞쪽으로 Ray를 쏘기
        if (Physics.Raycast(Feet.transform.position, Feet.transform.forward, out hit, 1f))
        {
            // 장애물의 높이 체크
            if (hit.transform.position.y >= (Feet.transform.position.y + 2) && hit.transform.position.y < (Feet.transform.position.y + 1)) // '2'는 넘을 수 있는 최대 높이
            {
                //장애물 넘기
                Vector3 jumpForce = new Vector3(0, CalculateJumpVerticalSpeed(), 0);
                rb_player.AddForce(jumpForce, ForceMode.Impulse);
            }
        }

    }

    private void Update()
    {
        float[] reciveFromWeapon = AssaultRifle.giveToPC();

        /*계속 덮어씌우는 것을 방지하기 위해 last를 관리 한다
        하지만 둘 다 0 일 떄, 값을 받아오지 않는 문제가 있기 때문에 0일 때는 무조건 받아오도록 한다.*/
        if(recoilAmount != reciveFromWeapon[0] || (recoilAmount == 0 && reciveFromWeapon[0] == recoilAmount))
        {
            recoilAmount = reciveFromWeapon[0];
            origin_recoilAmount = recoilAmount;
        }
        if(recoilRecoverySpeed != reciveFromWeapon[1] || (recoilRecoverySpeed == 0 && reciveFromWeapon[1] == recoilRecoverySpeed))
        {
            recoilRecoverySpeed = reciveFromWeapon[1];
            origin_recoilRecoverySpeed = recoilRecoverySpeed;
        }
        
        gm.HP_UI_Update(HP);

        if(NetworkServer.active && !gm.Time_isMinus())
        {
            Time_spent();
        }
    }

    public void Rebound()
    {
        // 반동을 부드럽게 적용한다.
        if (AssaultRifle.canShoot && !canFire)  //시간에 따른다
        {
            MouseY += currentRecoil * Time.deltaTime; // 시간에 따른 반동 적용
            currentRecoil -= recoilRecoverySpeed * Time.deltaTime; // 시간에 따른 반동 감소
            currentRecoil = Mathf.Max(currentRecoil, recoilAmount * 0.9f); // 반동이 0보다 작아지지 않도록 함
        }
        else // 총을 쏘지 않는 상태라면
        {
            if(currentRecoil != origin_recoilAmount)
                currentRecoil = origin_recoilAmount; // 반동을 원래의 값으로 복구

            if(recoilRecoverySpeed != origin_recoilRecoverySpeed)
                recoilRecoverySpeed = origin_recoilRecoverySpeed;
        }
    }

    // 점프 속도 계산
    float CalculateJumpVerticalSpeed()
    {
        // 물리식으로 점프 높이를 계산합니다 (높이 = 0.5 * 중력 * 점프시간^2)
        return Mathf.Sqrt(2 * 2 * Physics.gravity.magnitude);
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

                /* 이동 방향 계산
                   - transform.right * Horizontal_move: 수평 입력에 따른 이동 벡터
                   - new Vector3(transform.forward.x, 0, transform.forward.z) * Vertical_move: 수직 입력에 따른 이동 벡터, y 성분 0으로 설정하여 수평적으로만 움직임
                   - 두 이동 벡터 더한 후 .normalized로 크기 1로 만듦 -> 대각선 이동 시 속도 일정하게 유지 */
                Vector3 moveDirection = (transform.right * Horizontal_move + new Vector3(transform.forward.x, 0, transform.forward.z) * Vertical_move).normalized;


                // 최종 이동 벡터 계산.moveDirection에 이동 속도(speed)와 프레임 간 시간(Time.deltaTime) 곱함->일정한 속도로 움직임
                vec = moveDirection * speed * Time.deltaTime;

                if(!moving)
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
                isJump = true; //2중 점프 막기
                wasd = false;   //점프 중 움직임 막기

                // 월드 좌표를 기준으로 점프 벡터 설정
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

    [ClientRpc]
    public void Hitted_Bullet(int damage)   //CmdReduceHP의 외부접근을 위한 함수
    {
        CmdReduceHP(damage);
    }

    [Command]
    void CmdReduceHP(int damage)
    {
        if (HP > damage)
            HP -= damage;
        else
            HP = 0;
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

    /*IEnumerator Respawn()
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
    }*/

    IEnumerator Weapon_delay()
    {
        canFire = false;
        bc.Bullet_Shoot(rb_weapon, rb_player);

        Debug.Log(attackRate);

        yield return new WaitForSeconds(attackRate);  //임의값
        canFire = true;
    }

    /*[ClientRpc] //모든 클라이언트에서 서버의 명령을 받고 해당 플레이어를 부활로 처리
    void RpcRespawn()
    {
        //부활 및 좌표 변경
    }
*/
    private void Rotate()   //이렇게 하게되면 마우스에 따라 플레이어가 각도를 틀지만, 조금 더 자연스러운 움직임을 위해 머리 몸통을 나눠서 움직여야 할 듯?
    {
        if(alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;

            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -90f, 90f);    //위 아래 고개 최대 범위 -90 ~ 90
        }

        Quaternion quat = Quaternion.Euler(new Vector3(-MouseY, MouseX, 0));
        transform.rotation
            = Quaternion.Slerp(transform.rotation, quat, Time.fixedDeltaTime * MouseSen);
    }

    [Server]
    private void SpawnPlayer()  //서버적으로 각종 플레이어 수치들이 할당되어야 한다.
    {
        Vector3 Spawn_Point = new Vector3(0, 20, 0);
        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(55f, 0, -0.6140758f);
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

        bc.weapon = Attack_Object.GetComponentInChildren<WeaponAssaultRifle>();

        gm = GameManager.gm_instance;

        gm.UI_Init();   //UI 초기화

        moving = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)   //땅 혹은 지형지물에 다시 닿으면 점프 다시 활성화
        {
            if (collision.gameObject.tag == "land")
            {
                isJump = false;
                wasd = true;

                Debug.Log("땅에 착지: " + isJump + " // " + wasd);
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

    [Server]
    private void Time_spent()   //시간 흐르는 것 관련
    {
        float[] t_res = gm.Time_go();
        Time_Update(t_res[0], t_res[1]);
    }

    [ClientRpc]
    public void Time_Update(float m, float s)   //시간 UI Update
    {
        string sec = s < 10 ? "0" + s.ToString() : s.ToString();
        string min = m.ToString() + ":";

        gm.game_Time_UI.text = string.Format(min + sec);
    }

}
