using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/*
 * UI관련 및 Map 좌표 정리 등등...
 */
public class GameManager : MonoBehaviour
{
    public Image HP_bar;
    public TextMeshProUGUI HP_text;
    public Image Respawn_bar;

    public static GameManager gm_instance;

    private void Awake()
    {
        if(gm_instance == null)
        {
            gm_instance = this;
        }
        else
        {
            Debug.LogWarning("gm_instance already exists, destroying object!");
        }
    }

    public void HP_UI_Update(int hp)
    {
        HP_bar.fillAmount = hp / 100f;
        HP_text.text = string.Format("{0} / 100", hp);

        if(hp <= 0)
            Respawn_bar.gameObject.SetActive(true);
        else
            Respawn_bar.gameObject.SetActive(false);
    }

    public void Respawn_bar_Update(float time, float fullTime)
    {
        Respawn_bar.fillAmount = time / fullTime;   
    }
}
