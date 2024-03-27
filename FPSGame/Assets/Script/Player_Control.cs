using System.Collections;
using UnityEngine;
using Mirror;
using System;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update

    public bool alive;
    public bool isJump;
    public bool wasd;
    public bool moving;

    public float MouseX;
    public float MouseY;
    private Vector3 jumpDirection;

    [SyncVar]
    public int HP = 100;
    [SyncVar]
    public float lim_Speed;
    [SyncVar]
    public float jumpPower;
    [SyncVar]
    public float speed;
    [SyncVar]
    public float MouseSen;
    [SyncVar]
    public float Respawn_Time;
    [SyncVar]
    public float attackRate;

    public GameObject Head;
    public GameObject Arm;
    public WeaponAssaultRifle AssaultRifle;

    public float currentRecoil;
    public float recoilAmount;
    public float origin_recoilAmount;
    public float recoilRecoverySpeed;
    public float origin_recoilRecoverySpeed;

    public GameObject Attack_point;
    public Rigidbody rb_player;
    public Rigidbody rb_weapon;
    public GameObject Feet;

    [SerializeField]
    public Bullet_Control bc;

    [SerializeField]
    public GameManager gm;

    public Camera playerCam;
    public Camera mainCam;

    public NetworkIdentity playerIdentity { get; set; }


    [SyncVar]
    public bool canFire;
    private void Start()
    {
        AssaultRifle = GetComponentInChildren<WeaponAssaultRifle>();
        attackRate = AssaultRifle.weapon_attackRate();

        if(isLocalPlayer)
        {
            getInstance();
            Game_Setting(Attack_point);
        }

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
        if (Physics.Raycast(Feet.transform.position, Feet.transform.forward, out hit, 1f))
        {
            if (hit.transform.position.y >= (Feet.transform.position.y + 2) && hit.transform.position.y < (Feet.transform.position.y + 1))
            {
                Vector3 jumpForce = new Vector3(0, CalculateJumpVerticalSpeed(), 0);
                rb_player.AddForce(jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void Update()   //gm과 bc의 부재
    {
        if (!isLocalPlayer) return;

        if (gm == null || bc == null) getInstance();

        float[] reciveFromWeapon = AssaultRifle.giveToPC();

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

        if (gm != null)
            gm.HP_UI_Update(HP);
        else
            Debug.Log("Player_Control gm is null");

        if (bc != null)
            bc.getFromPC(Attack_point);
        else
            Debug.Log("Player_Control bc is null");
    }

    public void Rebound()   //반동함수
    {
        if(!isLocalPlayer) return;

        if (AssaultRifle.canShoot && !canFire)
        {
            MouseY += currentRecoil * Time.deltaTime;

            currentRecoil -= recoilRecoverySpeed * Time.deltaTime;
            currentRecoil = Mathf.Max(currentRecoil, recoilAmount * 0.9f);
        }
        else
        {
            if (currentRecoil != origin_recoilAmount)
                currentRecoil = origin_recoilAmount;
            if (recoilRecoverySpeed != origin_recoilRecoverySpeed)
                recoilRecoverySpeed = origin_recoilRecoverySpeed;
        }
    }

    float CalculateJumpVerticalSpeed() { return Mathf.Sqrt(2 * 2 * Physics.gravity.magnitude); }

    private void player_movement()  //플레이어 움직임 함수
    {
        if (!isLocalPlayer) return;

        Vector3 vec;

        if (alive)
        {
            if (wasd)
            {
                float Horizontal_move = Input.GetAxis("Horizontal");
                float Vertical_move = Input.GetAxis("Vertical");

                Vector3 forwardMovement = Head.transform.forward.normalized * Vertical_move;
                Vector3 rightMovement = Head.transform.right.normalized * Horizontal_move;
                Vector3 moveDirection = (forwardMovement + rightMovement).normalized;
                moveDirection.y = 0; // Y축 이동 제거, 플레이어가 상하로 움직이지 않도록 합니다.
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
                isJump = true;
                wasd = false;
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
                    Debug.Log("Movement Fire!"+isOwned);
                    CmdFire();
                }
            }
        }
    }

    public override void OnStartServer()
    {
        try
        {
            Debug.Log("Server Start!");
            if (isServer)
                SpawnPlayer();
        }
        catch (Exception e)
        {
            Debug.LogError(e + " Server is not activate");
        }

    }

    public override void OnStartClient()
    {
        if(isLocalPlayer)
        {
            Debug.Log("I'm Client!");
        }
    }

    [Command]
    void CmdFire()
    {
        try
        {
            if (!canFire) return;

            Debug.Log("CmdFire Shoot!");
            bc.Bullet_Shoot();
            canFire = false;
            StartCoroutine("Weapon_delay");

        }
        catch(Exception e)
        {
            if (bc == null)
                Debug.LogError("CmdFire bc is null");

            Debug.LogError(e.Message);
        }
        
    }

    IEnumerator Weapon_delay()
    {
        yield return new WaitForSeconds(attackRate);
        canFire = true;
    }

    //[ClientRpc]
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

    }

    /*IEnumerator Respawn()
    {
        float startTime = Time.time;
        float elapsedTime = 0;
        
        while (elapsedTime < Respawn_Time)
        {
            elapsedTime = Time.time - startTime;
            gm.Respawn_bar_Update(elapsedTime, Respawn_Time);
            yield return null;
        }
        
        if (isServer)
        {
            HP = 100;
            alive = true;
            RpcRespawn();
        }
    }*/

    private void Rotate()
    {
        if (!isLocalPlayer) return;

        if (alive)
        {
            MouseX += Input.GetAxisRaw("Mouse X") * MouseSen * Time.deltaTime;
            MouseY += Input.GetAxisRaw("Mouse Y") * MouseSen * Time.deltaTime;

            MouseY = Mathf.Clamp(MouseY, -70f, 70f);
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


    private void SpawnPlayer()
    {
        Debug.Log("SpawnPlayer 발동");

       /* Vector3 Spawn_Point = new Vector3(0, 20, 0);

        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(55f, 0, -0.6140758f);

        Attack_point.transform.localRotation = Quaternion.Euler(0, 0, 0);*/

        try
        {
            if (NetworkServer.active)
            {
                Debug.LogError("서버가 제대로 설계되었다");

                if (isServer)
                    OSOPRoomManager.GameStart();

            }
            else
            {
                Debug.LogError("NetworkServer is not active. Cannot spawn objects.");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

    }

    public void Game_Setting(GameObject Attack_Object)
    {
        canFire = true;

        rb_player = Attack_Object.GetComponent<Rigidbody>();
        rb_weapon = AssaultRifle.GetComponentInChildren<Rigidbody>();

        Scene gameScene = SceneManager.GetActiveScene();

        if(gameScene.name == "Gameplay")
        {
            mainCam = Camera.main;

            if (playerCam != null)
            {
                playerCam.gameObject.SetActive(true);
                playerCam.transform.SetParent(Head.transform); // Head는 해당 플레이어의 머리 오브젝트를 나타냅니다.
                playerCam.transform.localPosition = new Vector3(-0.6f, -0.6f, 0.3f);

                mainCam.depth = 0;
                playerCam.depth = 1;
            }
            else
            {
                Debug.Log("카메라를 찾을 수 없습니다.");
            }
        }

    }

    public void getInstance()
    {
        // GameManager가 준비될 때까지 기다리는 코루틴 시작
        StartCoroutine(WaitForGameManager());

        // BulletControl이 준비될 때까지 기다리는 코루틴 시작
        StartCoroutine(WaitForBulletControl());
    }
    private IEnumerator WaitForGameManager()
    {
        // GameManager의 인스턴스가 준비될 때까지 대기
        yield return new WaitUntil(() => GameManager.instance != null);

        // GameManager 인스턴스 할당
        gm = GameManager.instance;

        // 여기에 이어지는 로직 추가
        gm.UI_Init();
        moving = false;
    }

    private IEnumerator WaitForBulletControl()
    {
        // Bullet_Control 인스턴스가 준비될 때까지 대기
        yield return new WaitUntil(() => Bullet_Control.instance != null);

        // Bullet_Control 인스턴스 할당
        bc = Bullet_Control.instance;

        // 여기에 이어지는 로직 추가
        bc.weapon = AssaultRifle;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (alive)
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
            Vector3 player_pos = transform.position;
            Vector3 bullet_pos = other.transform.position;

            Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));

        }
    }

    public void Hitted_Bullet(int damage)   //CmdReduceHP의 외부접근을 위한 함수 피 깎는 함수
    {
        CmdReduceHP(damage);
    }

    void CmdReduceHP(int damage)
    {
        if (HP > damage)
            HP -= damage;
        else
            HP = 0;
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);
        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2));
    }

    /*[Server]
    private void Time_spent()
    {
        float[] t_res = gm.Time_go();
        Time_Update(t_res[0], t_res[1]);
    }
    
    [ClientRpc]
    public void Time_Update(float m, float s)
    {
        string sec = s < 10 ? "0" + s.ToString() : s.ToString();
        string min = m.ToString() + ":";
        
        gm.game_Time_UI.text = string.Format(min + sec);
    }    */
}