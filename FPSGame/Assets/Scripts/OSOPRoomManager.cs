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
        // 현재 연결된 모든 클라이언트가 준비되었는지 확인
        if (readyPlayers.Count == NetworkServer.connections.Count)
        {
            foreach (var conn in readyPlayers)
            {
                if (conn != null && conn.identity != null)
                {
                    var player = conn.identity.gameObject;
                    // NetworkIdentity의 속성들을 로그로 출력
                    Debug.Log($"Player Name: {player.name}, NetID: {conn.identity.netId}, IsLocalPlayer: {conn.identity.isLocalPlayer}");

                    // 플레이어의 위치를 설정합니다.
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
