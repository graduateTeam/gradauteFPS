using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OSOPRoomManager : NetworkRoomManager
{
    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerConnect(conn);
    }
}
