using System.Collections;
using UnityEngine;
using Mirror;
using System;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class Player_Control : NetworkBehaviour
{
    // Start is called before the first frame update
    public int PlayerNumber;

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
    private Bullet_Control bc;

    [SerializeField]
    private Bullet_Pool bp;

    [SerializeField]
    private GameManager gm;

    public Camera playerCam;
    public Camera mainCam;

    public NetworkIdentity playerIdentity { get; set; }

    [SyncVar]
    public bool canFire;

    [SyncVar]
    public Vector3 gunEndPos;

    [SyncVar]
    private float attackDis;

    [SyncVar]
    private float attackSpd;

    //0404 클라이언트의 bc와 gm이 여전히 null이다.
    private void Start()
    {
        AssaultRifle = GetComponentInChildren<WeaponAssaultRifle>();
        attackRate = AssaultRifle.weapon_attackRate();

        if (isLocalPlayer)
        {
            Game_Setting(Attack_point);
            InitializeWeapon();
        }

    }

    private void InitializeWeapon()
    {
        if (AssaultRifle != null)
        {
            rb_weapon = AssaultRifle.GetComponentInChildren<Rigidbody>();

            float[] receive = AssaultRifle.Gun_Info();
            attackDis = receive[0];
            attackSpd = receive[1];

            Renderer renderer = rb_weapon.GetComponent<Renderer>();
            float localZOffset = rb_weapon.transform.InverseTransformPoint(renderer.bounds.center).z;

            gunEndPos = rb_weapon.transform.position + rb_weapon.transform.forward * localZOffset;
        }
        else
        {
            Debug.LogError("AssaultRifle is null");
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

        if (gm == null)
            Debug.LogWarning("Player_Control gm is null");

        if (gm != null)
            gm.HP_UI_Update(HP);

        if (bc != null)
            bc.setWeaponInfo(Head, AssaultRifle);
        if (gm.PlayerCount == 1)
        {
            CmdplayerWins();
        }
    }

    public void Rebound()   //반동함수
    {
        if (!isLocalPlayer) return;

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
                if (isLocalPlayer && canFire && NetworkClient.ready)
                {
                    Debug.Log("Movement Fire!" + isOwned);
                    //CmdFire();
                    Bullet_Shoot();
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
        if (isLocalPlayer)
        {
            Debug.Log("I'm Client!");
            CmdRequestComponents();
            //NetworkClient.Ready();
        }
    }

    /*[Command]
    void CmdFire()  //일단 건들 일은 없어보임 0404
    {
        if (bc == null)
        {
            Debug.LogError("CmdFire: bc is null");
            return;
        }

        Debug.Log("CmdFire Shoot!");
        bc.Bullet_Shoot();
        canFire = false;
        StartCoroutine("Weapon_delay");
    }*/

    /*[Command]
    void CmdFire()
    {
        if (bc == null)
        {
            Debug.LogError("CmdFire: bc is null");
            return;
        }

        Debug.Log("CmdFire Shoot!");

        GameObject bullet = bc.Bullet_Shoot();

        if (bullet != null)
        {
            NetworkServer.Spawn(bullet);
        }

        canFire = false;
        RpcWeaponDelay();
        RpcDecreaseAmmoOnClients();
    }*/

    [Client]    //0418 총알발사 전면 교체 (주석처리 했음)
    public void Bullet_Shoot()
    {
        Debug.LogError("Bullet_Shoot!!");

        if (!isLocalPlayer || !canFire || !NetworkClient.ready) return;

        if (AssaultRifle == null || !AssaultRifle.canShoot) return;

        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;
        Ray oldray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        Ray ray = new Ray(rb_weapon.transform.position, Head.transform.rotation * oldray.direction);

        if (Physics.Raycast(ray, out hit, 1000))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * attackDis;
        }

        Debug.DrawRay(ray.origin, ray.direction * attackDis, Color.red);

        Vector3 attackDirection = Head.transform.forward;
        Quaternion lookDirection = Quaternion.LookRotation(attackDirection);
        Vector3 velocity = attackDirection * attackSpd;

        InitializeWeapon();

        CmdShootBullet(gunEndPos, lookDirection, velocity);

        canFire = false;
        StartCoroutine("Weapon_delay");
        AssaultRifle.UseAmmo();
    }

    [Command]
    void CmdShootBullet(Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (bp == null)
            Debug.LogError("총 발사 bp Error");

        GameObject bullet = bp.GetBullet(pos, rotation, velocity);

        if (bullet != null)
        {
            NetworkServer.Spawn(bullet);
            RpcSyncBullet(bullet.GetComponent<NetworkIdentity>().netId, pos, rotation, velocity);
        }
    }

    //추가된 부분 0410
    [ClientRpc]
    public void RpcSyncBullet(uint bulletNetId, Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        if (NetworkClient.spawned.TryGetValue(bulletNetId, out NetworkIdentity bulletIdentity))
        {
            GameObject bullet = bulletIdentity.gameObject;
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.GetComponent<Rigidbody>().velocity = velocity;

            Debug.LogError("RpcSyncBullet position: " + bullet.transform.position + " // rotation: " + bullet.transform.rotation
                + " // velocity: " + bullet.GetComponent<Rigidbody>().velocity);
        }
        else
        {
            Debug.LogWarning($"Bullet with netId {bulletNetId} not found in NetworkClient.spawned");
        }
    }


    IEnumerator Weapon_delay()
    {
        yield return new WaitForSeconds(attackRate);
        canFire = true;
    }

    
    void CmdplayerDies()
    {
        alive = false;
        HP = 0;
        RpcplayerDies();
        //StartCoroutine("Respawn");
    }
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void RpcplayerDies()
    {
        //gm.HP_UI_Update(HP);
        this.gameObject.SetActive(false);
        LoadSceneByName("Loser");
        Cursor.lockState = CursorLockMode.None;
    }
    void CmdplayerWins()
    {
        this.gameObject.SetActive(false);
        LoadSceneByName("Winner");
        Cursor.lockState= CursorLockMode.None;
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
        try
        {
            if (NetworkServer.active && NetworkClient.ready)
            {
                if (isServer)
                {
                    OSOPRoomManager.GameStart();
                }


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

        if (gameScene.name == "Gameplay")
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

    /*[Command]
    private void CmdGetInstances()
    {
        if (GameManager.instance != null && Bullet_Control.instance != null)
        {
            RpcSetInstances(GameManager.instance, Bullet_Control.instance);
        }
    }

    [ClientRpc]
    private void RpcSetInstances(GameManager gameManagerInstance, Bullet_Control bulletControlInstance)
    {
        gm = gameManagerInstance;
        bc = bulletControlInstance;

        // 인스턴스를 받은 후에 필요한 로직 수행
        gm.UI_Init();
        moving = false;
        bc.weapon = AssaultRifle;
    }*/
    [Command]
    private void CmdRequestComponents()
    {
        if (GameManager.instance != null && Bullet_Control.instance != null)
        {
            GameManager.instance.setPlayerControl(this);
            Bullet_Control.instance.setPlayerControl(this);
            Bullet_Pool.instance.setPlayerControl(this);
            RpcSetComponents();
        }
    }

    [ClientRpc]
    private void RpcSetComponents()
    {
        if (!isServer)
        {
            gm = GameManager.instance;
            bc = Bullet_Control.instance;
        }
    }

    public void setGameManager(GameManager gameManager)
    {
        gm = gameManager;
        gm.UI_Init();
    }

    public void setBulletControl(Bullet_Control bulletControl)
    {
        bc = bulletControl;
        bc.weapon = AssaultRifle;
    }

    public void setBulletPool(Bullet_Pool bulletPool)
    {
        bp = bulletPool;
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

            Hitted_Bullet(10/*damage_Reducing(player_pos, bullet_pos, 10)*/);
            gm.HitDamage.SetActive(true);
            // 코루틴 시작
            StartCoroutine(DeactivateAfterDelay(5.0f));
        }
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        // 지정된 시간(초)만큼 대기
        yield return new WaitForSeconds(delay);

        // 대기 시간이 끝난 후 실행할 코드
        gm.HitDamage.SetActive(false);
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
        {
            HP = 0;

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

    public void setPlayerNumber(OSOPRoomManager osop)
    {
        PlayerNumber = osop.playerCount();

        Debug.LogWarning(PlayerNumber);
    }
}