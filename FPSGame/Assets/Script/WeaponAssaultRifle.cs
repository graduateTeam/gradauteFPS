using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
public class WeaponAssaultRifle : MonoBehaviour
{
    public Weapon_Info WeaponInfo { get; set; }

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

        GameManager.instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        GameManager.instance.getGunLabel(WeaponInfo.weaponName);

        canShoot = true;
    }

    public float[] giveToPC()
    {
        float[] temp = { weaponInfo.recoilAmount, weaponInfo.recoilRecoverySpeed };
        return temp;
    }

    public float weapon_attackRate()
    {
        return weaponInfo.attackRate;
    }

    public float[] Gun_Info()
    {
        float[] temp = { weaponInfo.attackDistance, weaponInfo.bulletSpeed };
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
        //PlaySound(audioClipTakeOutWeapon);

        onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

        if (this != null)
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
            onAmmoEvent.RemoveListener(OnAmmoCharged);
        }
    }

    private void OnAmmoCharged(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo <= 0)
        {
            StartCoroutine("OnReload");
        }
    }
    public void UseAmmo()
    {
        if (CurrentAmmo > 0 && canShoot)
        {
            weaponInfo.currentAmmo--;
            onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

            GameManager.instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
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

        yield return new WaitForSeconds(2f);

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
        GameManager.instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);


        isReload = false;
        canShoot = true;


        /*while (true)
        {
            if(true)
            {
                

                yield break;
            }
        }*/
    }
}