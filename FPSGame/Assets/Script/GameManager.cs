using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;
/*
* UI관련 및 Map 좌표 정리 등등...
*/
public class GameManager : NetworkBehaviour
{
    public Image HP_bar;
    public TextMeshProUGUI HP_text;
    //public Image Respawn_bar;
    public Image ImageAim;

    public static GameManager instance;

    public TextMeshProUGUI game_Time_UI;

    private int Red_kill;
    private int Blue_kill;
    private float gameTime;
    private float minute;
    private float second;

    [Header("Components")]
    [SerializeField]
    private WeaponAssaultRifle weapon;

    [Header("Magazine")]
    [SerializeField]
    private Transform magazineParent;   //탄창 UI가 배치되는 Panel
    [SerializeField]
    private TextMeshProUGUI AmmoText;
    [SerializeField]
    private TextMeshProUGUI gunLabel;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Multiple instances of GameManager detected.");
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void AimPos(Vector3 pos)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(pos);
        if (!(viewportPos.z > 0 && viewportPos.x >= 0) && !(viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
            ImageAim.transform.position = new Vector3(screenPos.x, screenPos.y, ImageAim.transform.position.z);
            Vector3 cameraPos = Camera.main.transform.position;

            Camera.main.transform.position = new Vector3(cameraPos.x, ImageAim.transform.position.y, cameraPos.z);

        }
    }


    public void UpdateMagazineHUD(int currentAmmo, int maximum)
    {
        AmmoText.text = string.Format("{0} / {1}", currentAmmo, maximum);
    }

    public void getGunLabel(WeaponName weaponName)
    {
        gunLabel.text = weaponName.ToString();
    }

    public void HP_UI_Update(int hp)
    {
        if(HP_bar.gameObject.activeSelf)
        {
            HP_bar.fillAmount = hp / 100f;
            HP_text.text = string.Format("{0} / 100", hp);
        }

        /*if(hp <= 0)
            Respawn_bar.gameObject.SetActive(true);
        else
            Respawn_bar.gameObject.SetActive(false);*/
    }

    public void Respawn_bar_Update(float time, float fullTime)
    {
        //Respawn_bar.fillAmount = time / fullTime;   
    }

    public void UI_Init()
    {
        gameTime = 300;

        minute = 5;
        second = 0;

        Red_kill = 0;
        Blue_kill = 0;
    }

    public float[] Time_go()    //시간 가기
    {
        gameTime -= Time.deltaTime;

        minute = gameTime / 60;
        second = gameTime % 60;

        minute = Mathf.Floor(minute);
        second = Mathf.Floor(second);

        return new float[] {minute, second};
    }

    public int[] Who_kill(string team) //누군가 죽이면 킬 수 올리기
    {
        if(team.Equals("red"))
        {
            Red_kill += 1;
        }
        else
        {
            Blue_kill += 1;
        }

        return new int[] {Red_kill, Blue_kill};
    }

    public Boolean Time_isMinus()
    {
        if (gameTime < 1) 
            return true;
        else
            return false;
    }
}

