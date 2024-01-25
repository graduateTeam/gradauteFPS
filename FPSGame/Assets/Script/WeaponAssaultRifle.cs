using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
public class WeaponAssaultRifle : MonoBehaviour
{
    public Weapon_Info WeaponInfo { get; set; }

    private Bullet_Control bc;

    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();

    [Header("Weapon_Info")]
    [SerializeField]
    private Weapon_Info weaponInfo;

    public int CurrentAmmo => weaponInfo.currentAmmo;
    public int MaxAmmo => weaponInfo.maxAmmo;

    public int wholeAmmo => weaponInfo.wholeAmmo;

    private bool isReload = false;
    public bool canShoot { get; set; }

    private void Awake()
    {
        //처음 탄약 수는 최대로 설정
        weaponInfo.currentAmmo = weaponInfo.maxAmmo;

        GameManager.gm_instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        GameManager.gm_instance.getGunLabel(WeaponInfo.weaponName);

        canShoot = true;
        bc = Bullet_Control.bc_instance;
    }

    public float[] giveToPC()
    {
        float[] temp = {weaponInfo.recoilAmount, weaponInfo.recoilRecoverySpeed};
        return temp;
    }

    public float weapon_attackRate()
    {
        return weaponInfo.attackRate;
    }

    private void Start()
    {
        bc.get_Info(weaponInfo.attackDistance, weaponInfo.bulletSpeed);
    }

    private void Update()
    {
        //무기 액션 도중에 'R'키를 눌러 재장전을 시도하면 무기 액션 종료 후 재장전 Reload인식을 못함
        if (Input.GetKeyDown(KeyCode.R))
        {
            //현재 재장전 중이거나 탄약 수가 0이면 재장전 불가
            if (isReload || wholeAmmo <= 0) return;

            Debug.Log("GetKeyDown Reload!");
            StartCoroutine("OnReload");
        }
    }

    private void OnEnable()
    {
        //무기가 활성화될 때 해당 무기의 탄약 수 정보를 갱신한다.
        onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

        if(this != null)
        {
            onAmmoEvent.AddListener(OnAmmoCharged);
        }
    }

    private void OnDisable()
    {
        if (this != null)
        {
            Debug.Log("이벤트 해제");
            onAmmoEvent.RemoveListener(OnAmmoCharged);
        }
    }

    private void OnAmmoCharged(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("탄약이 없습니다.");
            StartCoroutine("OnReload");
        }
    }
    public void UseAmmo()
    {
        if(CurrentAmmo > 0 && canShoot)
        {
            weaponInfo.currentAmmo--;
            onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

            GameManager.gm_instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        }
        else
        {
            canShoot = false;
        }
    }

    

    private IEnumerator OnReload()
    {
        isReload = true;
        canShoot = false;

        yield return new WaitForSeconds(2f); // 2초 동안 대기

        //재장전 애니메이션, 사운드 재생

        isReload = false;
        canShoot = true;

        //현재 탄약 수를 최대로 설정하고, 바뀐 탄약 수 정보를 Text UI에 업데이트
        int addAmmo = (weaponInfo.maxAmmo - weaponInfo.currentAmmo);

        if (addAmmo > weaponInfo.wholeAmmo)
        {
            weaponInfo.currentAmmo += weaponInfo.wholeAmmo;
            weaponInfo.wholeAmmo = 0;
        }
        else
        {
            weaponInfo.currentAmmo = weaponInfo.maxAmmo;
            weaponInfo.wholeAmmo -= addAmmo;
        }

        onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        GameManager.gm_instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

        /*while (true)
        {
            //조건문에 사운드가 재생중이 아니고, 현재 애니메이션이 Movement이면
            //재장전 애니메이션(, 사운드) 재생이 종료되었다는 의미
            if(true)
            {
                

                yield break;
            }
        }*/
    }
}
