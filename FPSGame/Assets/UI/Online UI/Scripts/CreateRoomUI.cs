using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CreateRoomUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateRoom()
    {
        var manager = OSOPRoomManager.singleton;
        // �� ���� �۾� ó��
        //
        //
        manager.StartHost();
    }
}
public class CreateGameRoomData
{
    
}
