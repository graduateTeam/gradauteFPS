using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetManager : NetworkManager
{
    // 플레이어가 준비되었는지 확인하기 위한 리스트
    private static List<NetworkConnectionToClient> readyPlayers = new List<NetworkConnectionToClient>();

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // 클라이언트가 서버에 연결될 때 실행될 코드
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        // 플레이어 스폰 로직
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        // 클라이언트가 준비되었다는 것을 확인하고 리스트에 추가
        if (!readyPlayers.Contains(conn))
        {
            readyPlayers.Add(conn);
        }

        // 모든 플레이어가 준비되었는지 체크하고 게임 시작
        CheckAllPlayersReady();
    }

    public static void CheckAllPlayersReady()
    {
        // 현재 연결된 모든 클라이언트가 준비되었는지 확인
        if (readyPlayers.Count == NetworkServer.connections.Count)
        {
            // 모든 플레이어가 준비되었으므로 게임 시작 로직 실행
            StartGame();
        }
    }

    public static void StartGame()
    {
        // 게임 시작 로직
        // 예: 모든 플레이어에게 게임이 시작되었다는 것을 알림, 필요한 게임 데이터 초기화 등
        Debug.Log("게임 시작!");

        // 예시: 모든 플레이어를 특정 위치로 이동시키기
        foreach (var conn in readyPlayers)
        {
            var player = conn.identity.gameObject;
            player.transform.position = new Vector3(0,20,0);
            // 추가적인 플레이어 초기화 로직
        }
    }
}
