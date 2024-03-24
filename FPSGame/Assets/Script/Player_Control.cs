using System.Collections;
using UnityEngine;
using Mirror;
using System;
using JetBrains.Annotations;

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
    public int HP;
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

    public bool canFire;

    //NetworkServer가 제대로 세워지지 않는게 gm과 bc의 부재로 보인다.
    private void Start()
    {
        AssaultRifle = GetComponentInChildren<WeaponAssaultRifle>();
        attackRate = AssaultRifle.weapon_attackRate();
        bc = GetComponent<Bullet_Control>();
        gm = GetComponent<GameManager>();

        getInstance();
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
        /*if (NetworkServer.active && !gm.Time_isMinus())
        {
            //Time_spent();            
        }*/

        if (bc != null)
            bc.getFromPC(Attack_point);
    }

    public void Rebound()   //반동함수
    {
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
                Vector3 moveDirection = (transform.right * Horizontal_move + new Vector3(Head.transform.forward.x, 0, Head.transform.forward.z) * Vertical_move).normalized;
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
            SpawnPlayer();
        }

        /*if (isLocalPlayer && NetworkClient.ready)
        {
            Debug.Log("I'm Client!");
            SpawnPlayer();
        }

        if (isLocalPlayer && !NetworkClient.ready)
        {
            NetworkClient.Ready();
        }*/
    }

    [Command]
    public void Hitted_Bullet(int damage)   //CmdReduceHP의 외부접근을 위한 함수 피 깎는 함수
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

    [Command]
    void CmdFire()
    {
        try
        {
            StartCoroutine("Weapon_delay");
        }
        catch (Exception e)
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
        //StartCoroutine("Respawn");
    }

    [ClientRpc]
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

    IEnumerator Weapon_delay()
    {
        canFire = false;

        try
        {
            bc.Bullet_Shoot();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        yield return new WaitForSeconds(attackRate);
        canFire = true;
    }

    /*[ClientRpc]
    void RpcRespawn()
    {
    
    }*/
    private void Rotate()
    {
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

        Vector3 Spawn_Point = new Vector3(0, 20, 0);

        Attack_point.transform.position = Spawn_Point;
        Attack_point.transform.Translate(55f, 0, -0.6140758f);

        Attack_point.transform.localRotation = Quaternion.Euler(0, 0, 0);

        try
        {
            if (NetworkServer.active)
            {
                Debug.LogError("서버가 제대로 설계되었다");
                Game_Start(Attack_point);

                NetManager.CheckAllPlayersReady(Spawn_Point);

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

    private void Game_Start(GameObject Attack_Object)
    {
        canFire = true;

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

        Camera cam = GetComponentInChildren<Camera>();

        Debug.Log("나의 카메라: " + cam);

        cam.transform.SetParent(Head.transform);
        cam.transform.localPosition = new Vector3(-0.6f, -0.6f, 0.3f);

    }

    private void getInstance()
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