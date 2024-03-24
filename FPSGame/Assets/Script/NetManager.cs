using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetManager : NetworkManager
{
    // �÷��̾ �غ�Ǿ����� Ȯ���ϱ� ���� ����Ʈ
    private static List<NetworkConnectionToClient> readyPlayers = new List<NetworkConnectionToClient>();

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        // Ŭ���̾�Ʈ�� ������ ����� �� ����� �ڵ�
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        // �÷��̾� ���� ����
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        // Ŭ���̾�Ʈ�� �غ�Ǿ��ٴ� ���� Ȯ���ϰ� ����Ʈ�� �߰�
        if (!readyPlayers.Contains(conn))
        {
            readyPlayers.Add(conn);
        }

        // ��� �÷��̾ �غ�Ǿ����� üũ�ϰ� ���� ����
        CheckAllPlayersReady();
    }

    public static void CheckAllPlayersReady()
    {
        // ���� ����� ��� Ŭ���̾�Ʈ�� �غ�Ǿ����� Ȯ��
        if (readyPlayers.Count == NetworkServer.connections.Count)
        {
            // ��� �÷��̾ �غ�Ǿ����Ƿ� ���� ���� ���� ����
            StartGame();
        }
    }

    public static void StartGame()
    {
        // ���� ���� ����
        // ��: ��� �÷��̾�� ������ ���۵Ǿ��ٴ� ���� �˸�, �ʿ��� ���� ������ �ʱ�ȭ ��
        Debug.Log("���� ����!");

        // ����: ��� �÷��̾ Ư�� ��ġ�� �̵���Ű��
        foreach (var conn in readyPlayers)
        {
            var player = conn.identity.gameObject;
            player.transform.position = new Vector3(0,20,0);
            // �߰����� �÷��̾� �ʱ�ȭ ����
        }
    }
}
