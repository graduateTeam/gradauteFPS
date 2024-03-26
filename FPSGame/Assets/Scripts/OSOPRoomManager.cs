using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OSOPRoomManager : NetworkRoomManager
{
    // �÷��̾ �غ�Ǿ����� Ȯ���ϱ� ���� ����Ʈ
    private static List<NetworkConnectionToClient> readyPlayers = new List<NetworkConnectionToClient>();

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        // �÷��̾� ���� ����
        if (!readyPlayers.Contains(conn))
        {
            readyPlayers.Add(conn);
        }
    }

    public static void GameStart()
    {
        // ���� ����� ��� Ŭ���̾�Ʈ�� �غ�Ǿ����� Ȯ��
        if (readyPlayers.Count == NetworkServer.connections.Count)
        {
            foreach (var conn in readyPlayers)
            {
                if (conn != null && conn.identity != null)
                {
                    var player = conn.identity.gameObject;
                    // NetworkIdentity�� �Ӽ����� �α׷� ���
                    Debug.Log($"Player Name: {player.name}, NetID: {conn.identity.netId}, IsLocalPlayer: {conn.identity.isLocalPlayer}");

                    // �÷��̾��� ��ġ�� �����մϴ�.
                    player.transform.position = new Vector3(0, 0, 0);
                }
                else
                {
                    Debug.LogError("Connection or NetworkIdentity is null");
                }
            }
        }
    }
}
