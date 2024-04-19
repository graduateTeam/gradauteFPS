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
        if (IsAllPlayersReady())
        {
            Debug.LogError("I'm Server activate Game!");
            // �÷��̾� ��ġ ����
            SetAllPlayersPosition();

            // ������Ʈ Ǯ�� Ȱ��ȭ
            ActivateObjectPooling();
        }
    }

    private static bool IsAllPlayersReady()
    {
        return readyPlayers.Count == NetworkServer.connections.Count;
    }

    private static void SetAllPlayersPosition()
    {
        foreach (var conn in readyPlayers)
        {
            if (conn != null && conn.identity != null)
            {
                var player = conn.identity.gameObject;
                player.transform.position = new Vector3(0, 0, 0); // ��ġ ���� ����
                Debug.Log($"Player Name: {player.name}, NetID: {conn.identity.netId}, IsLocalPlayer: {conn.identity.isLocalPlayer}");
            }
            else
            {
                Debug.LogError("Connection or NetworkIdentity is null");
            }
        }
    }

    private static void ActivateObjectPooling()
    {
        OSOPRoomManager osop = new OSOPRoomManager();
        foreach (var prefab in NetworkRoomManager.singleton.spawnPrefabs)
        {
            osop.SerBulletManager(prefab);
        }
    }

    [Server]
    public void SerBulletManager(GameObject bulletManager)
    {
        if(bulletManager.name.Equals("Bullet Manager"))
        {
            GameObject obj = Instantiate(bulletManager);
            NetworkServer.Spawn(obj);

            //obj.GetComponent<Bullet_Pool>().pool_spawn();
        }
        else
        {
            GameObject obj = Instantiate(bulletManager, new Vector3(100, 0, 100), Quaternion.Euler(0, 0, 0));
            NetworkServer.Spawn(obj);
        }
    }

}
