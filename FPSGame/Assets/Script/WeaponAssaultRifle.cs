using System.Collections;
using UnityEngine;

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

        canShoot = true;
    }

    public float[] giveToPC()
    {
        float[] temp = {weaponInfo.recoilAmount, weaponInfo.recoilRecoverySpeed};
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
        //���Ⱑ Ȱ��ȭ�� �� �ش� ������ ź�� �� ������ �����Ѵ�.
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
