/*
 * �ѱ�� ���õ� ������ ��Ƴ��� ��ũ��Ʈ
 * 
 */

public enum WeaponName { AssaultRifle = 0 }

[System.Serializable]
public struct Weapon_Info
{
    public WeaponName weaponName;   //�����̸�
    public int wholeAmmo;   //�� �Ѿ��� ����
    public int currentAmmo; //���� ź�� ��
    public int maxAmmo; //�ִ� ź�� ��
    public float attackRate;    //���� �ӵ�
    public float attackDistance;   // ���� ��Ÿ�
    public bool isAutomaticAttack;  // ���� ����
    public float recoilAmount;// �ݵ��� ��
    public float recoilRecoverySpeed;// �ݵ� ȸ�� �ӵ�
}

