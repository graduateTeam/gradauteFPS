using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * �ѱ�� ���õ� ������ ��Ƴ��� ��ũ��Ʈ
 * 
 */
public class Weapon_Info : MonoBehaviour
{
    public float delay;    //������
    public int Max_Bullet; //�Ѿ� �ѷ�
    public int One_Bullet; //�� źâ �Ѿ�

    public float recoilAmount = 1.0f; // �ݵ��� ��
    public float recoilRecoverySpeed = 3.0f; // �ݵ� ȸ�� �ӵ�
    public float currentRecoil = 0.0f; // ���� �ݵ� ����

    public string Gun_Name; //�� �̸�
    public float speed; //ź��
    public float damage;    //�����


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
