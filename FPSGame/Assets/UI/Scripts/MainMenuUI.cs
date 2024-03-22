using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI IP;
    public string IPv6;
    public string IPv4;

    private void Start()
    {
        IPAddress[] ipa = Dns.GetHostAddresses(Dns.GetHostName());

        for(int i = 0; i < ipa.Length; i++)
        {
            if (ipa[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPv4 = ipa[i].ToString();
            }
        }

        IP.text = "v4:\n" + IPv4;
    }

    public void OnClickOnlineButton()
    {
        Debug.Log("Click Online");
    }    
    public void OnClickQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
