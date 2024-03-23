using System.Collections;
using System.Runtime.CompilerServices;
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


    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;

    private AudioSource audioSource;

    public int CurrentAmmo => weaponInfo.currentAmmo;
    public int MaxAmmo => weaponInfo.maxAmmo;

    public int wholeAmmo => weaponInfo.wholeAmmo;

    private bool isReload = false;
    public bool canShoot { get; set; }

    private void Awake()
    {
        weaponInfo.currentAmmo = weaponInfo.maxAmmo;

        GameManager.gm_instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        GameManager.gm_instance.getGunLabel(WeaponInfo.weaponName);

        canShoot = true;
        bc = Bullet_Control.bc_instance;

        audioSource = GetComponent<AudioSource>();
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

    public float[] Gun_Info()
    {
        float[] temp = {weaponInfo.attackDistance, weaponInfo.bulletSpeed};
        return temp;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isReload || wholeAmmo <= 0) return;

            Debug.Log("GetKeyDown Reload!");
            StartCoroutine("OnReload");
        }
    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);

        onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

        if(this != null)
        {
            onAmmoEvent.AddListener(OnAmmoCharged);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void OnDisable()
    {
        if (this != null)
        {
            Debug.Log("총알 발사 불가");
            onAmmoEvent.RemoveListener(OnAmmoCharged);
        }
    }

    private void OnAmmoCharged(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("재장전!");
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

        Debug.Log("������ ��!");

        yield return new WaitForSeconds(2f);

        isReload = false;
        canShoot = true;

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
            if(true)
            {
                

                yield break;
            }
        }*/
    }
}
