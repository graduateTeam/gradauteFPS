using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet_Control : NetworkBehaviour
{
    public Bullet_Pool bp;

    public static Bullet_Control bc_instance;   //�̱��� ������� ������Ʈ ����
    public Player_Control player;
    // Start is called before the first frame update
    void Awake()
    {
        bp = Bullet_Pool.bp_instance;

        if(bc_instance == null )
        {
            bc_instance = this;
        }
        else
        {
            Debug.LogWarning("bc_Instance already exists, destroying object!");
            //Destroy(this);    //���� �� ó���� ��Ȱ��ȭ �Ǿ��ִ� �Ѿ� Ư���� ��ũ��Ʈ�� �ν����� â���� ����� �� �ִ�.
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("�Ѿ��� �浹");
        if (player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //�÷��̾ �� �ִ� ��ġ
            Vector3 bullet_pos = transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //���Ƿ� ����� 10�̶�� �� �� ��ũ��Ʈ�� ������� �������� �� ������� �ٲ� ��
        }

        if (collision.gameObject.tag != "bullet")    //�Ѿ˳��� �ε����� ������� ��츦 ����
            bp.ReturnBullet(this.gameObject);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100 - dis) / 100), 2)); //������ ��� ����� ���� �߽����κ��� �Ÿ��� ���� ����� �氨�� ���� ������
    }

    public void Bullet_Shoot(Rigidbody gun)
    {
        GameObject bullet = bp.GetBullet();

        Collider collider = gun.GetComponent<Collider>();
        float gunLength = collider.bounds.size.z;

        Vector3 b_vec = gun.transform.forward;  //���� �պκа� ���̰� ����µ� �����̰� ����?
        bullet.transform.position = gun.transform.position + (b_vec * gunLength * 2);

        bullet.transform.rotation = Quaternion.LookRotation(b_vec);
        bullet.GetComponent<Rigidbody>().AddForce(b_vec * 100);
    }
}
