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
    private AudioClip audioClipTakeOutWeapon;   // ���� ���� ����

    private AudioSource audioSource;            // ���� ��� ������Ʈ

    public int CurrentAmmo => weaponInfo.currentAmmo;
    public int MaxAmmo => weaponInfo.maxAmmo;

    public int wholeAmmo => weaponInfo.wholeAmmo;

    private bool isReload = false;
    public bool canShoot { get; set; }

    private void Awake()
    {
        //ó�� ź�� ���� �ִ�� ����
        weaponInfo.currentAmmo = weaponInfo.maxAmmo;

        GameManager.gm_instance.UpdateMagazineHUD(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);
        GameManager.gm_instance.getGunLabel(WeaponInfo.weaponName);

        canShoot = true;
        bc = Bullet_Control.bc_instance;

        audioSource = GetComponent<AudioSource>();
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
        float[] temp = {weaponInfo.attackDistance, weaponInfo.bulletSpeed};
        return temp;
    }

    private void Update()
    {
        //���� �׼� ���߿� 'R'Ű�� ���� �������� �õ��ϸ� ���� �׼� ���� �� ������ Reload�ν��� ����
        if (Input.GetKeyDown(KeyCode.R))
        {
            //���� ������ ���̰ų� ź�� ���� 0�̸� ������ �Ұ�
            if (isReload || wholeAmmo <= 0) return;

            Debug.Log("GetKeyDown Reload!");
            StartCoroutine("OnReload");
        }
    }

    private void OnEnable()
    {
        //���� ���� ���� ���
        PlaySound(audioClipTakeOutWeapon);

        //���Ⱑ Ȱ��ȭ�� �� �ش� ������ ź�� �� ������ �����Ѵ�.
        onAmmoEvent.Invoke(weaponInfo.currentAmmo, weaponInfo.wholeAmmo);

        if (this != null)
        {
            onAmmoEvent.AddListener(OnAmmoCharged);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();             // ������ ������� ���带 �����ϰ�,
        audioSource.clip = clip;        // ���ο� ���� clip���� ��ü ��
        audioSource.Play();             // ���� ���
    }

    private void OnDisable()
    {
        if (this != null)
        {
            Debug.Log("�̺�Ʈ ����");
            onAmmoEvent.RemoveListener(OnAmmoCharged);
        }
    }

    private void OnAmmoCharged(int currentAmmo, int maxAmmo)
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("ź���� �����ϴ�.");
            StartCoroutine("OnReload");
        }
    }
    public void UseAmmo()
    {
        if (CurrentAmmo > 0 && canShoot)
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

        yield return new WaitForSeconds(2f); // 2�� ���� ���

        //������ �ִϸ��̼�, ���� ���

        isReload = false;
        canShoot = true;

        //���� ź�� ���� �ִ�� �����ϰ�, �ٲ� ź�� �� ������ Text UI�� ������Ʈ
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
            //���ǹ��� ���尡 ������� �ƴϰ�, ���� �ִϸ��̼��� Movement�̸�
            //������ �ִϸ��̼�(, ����) ����� ����Ǿ��ٴ� �ǹ�
            if(true)
            {
                

                yield break;
            }
        }*/
    }
}