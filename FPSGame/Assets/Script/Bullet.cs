using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        bullet = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Player_Control player = collision.gameObject.GetComponent<Player_Control>();

        if(player != null)
        {
            Vector3 player_pos = player.Attack_point.transform.position;  //�÷��̾ �� �ִ� ��ġ
            Vector3 bullet_pos = bullet.transform.position;

            player.Hitted_Bullet(damage_Reducing(player_pos, bullet_pos, 10));   //���Ƿ� ����� 10�̶�� �� �� ��ũ��Ʈ�� ������� �������� �� ������� �ٲ� ��
        }

        Bullet_Pool pool = GetComponent<Bullet_Pool>();
        pool.ReturnBullet(bullet);
    }

    int damage_Reducing(Vector3 player_pos, Vector3 bullet_pos, int damage)
    {
        float dis = Vector3.Magnitude(bullet_pos - player_pos);

        return (int)(damage * Math.Round((float)((100-dis) / 100),2)); //������ ��� ����� ���� �߽����κ��� �Ÿ��� ���� ����� �氨�� ���� ������
    }
}
