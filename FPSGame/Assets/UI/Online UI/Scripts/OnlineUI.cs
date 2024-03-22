using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class OnlineUI : MonoBehaviour
{
    [SerializeField]
    private InputField nicknameInputField;
    //[SerializeField]
    //private GameObject createRoomUI;

    public void OnClickCreateRoomButton()
    {
        if(nicknameInputField.text != "")
        {
            PlayerSettings.nickname = nicknameInputField.text;
            //createRoomUI.SetActive(true);
            gameObject.SetActive(false);

            var manager = OSOPRoomManager.singleton;
            manager.StartHost();
        }
        else
        {
            nicknameInputField.GetComponent<Animator>().SetTrigger("on");
        }
    }
    public void OnClickEnterGameRoomButton()
    {
        if (nicknameInputField.text != "")
        {
            try
            {
                var manager = OSOPRoomManager.singleton;
                manager.StartClient();
            }catch(Exception e)
            {
                Debug.Log("����: " + e);
            }
        }
        else
        {
            nicknameInputField.GetComponent<Animator>().SetTrigger("on");
        }
    }
}
