using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 총기와 관련된 정보를 담아놓은 스크립트
 * 
 */
public class Weapon_Info : MonoBehaviour
{
    public float delay;    //딜레이
    public int Max_Bullet; //총알 총량
    public int One_Bullet; //한 탄창 총알

    public float recoilAmount = 1.0f; // 반동의 양
    public float recoilRecoverySpeed = 3.0f; // 반동 회복 속도
    public float currentRecoil = 0.0f; // 현재 반동 상태

    public string Gun_Name; //총 이름
    public float speed; //탄속
    public float damage;    //대미지


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
