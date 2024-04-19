using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OSOPRoomManager : NetworkRoomManager
{
    // 플레이어가 준비되었는지 확인하기 위한 리스트
    private static List<NetworkConnectionToClient> readyPlayers = new List<NetworkConnectionToClient>();

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        // 플레이어 스폰 로직
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
            // 플레이어 위치 설정
            SetAllPlayersPosition();

            // 오브젝트 풀링 활성화
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
                player.transform.position = new Vector3(0, 0, 0); // 위치 설정 로직
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
