/*
 * 총기와 관련된 정보를 담아놓은 스크립트
 * 
 */

public enum WeaponName { AssaultRifle = 0 }

[System.Serializable]
public struct Weapon_Info
{
    public WeaponName weaponName;   //무기이름
    public int wholeAmmo;   //총 총알의 갯수
    public int currentAmmo; //현재 탄약 수
    public int maxAmmo; //최대 탄약 수
    public float attackRate;    //공격 속도
    public float attackDistance;   // 공격 사거리
    public bool isAutomaticAttack;  // 연발 여부
    public float recoilAmount;// 반동의 양
    public float recoilRecoverySpeed;// 반동 회복 속도
}

