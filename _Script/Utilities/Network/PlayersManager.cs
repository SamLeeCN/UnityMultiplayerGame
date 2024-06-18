using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
public class PlayersManager : NetworkSingleton<PlayersManager>
{
    private NetworkVariable<int> playersNumInGame = new NetworkVariable<int>();

    public int PlayersNumInGame
    {
        get { return playersNumInGame.Value;}
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if(IsServer)
            {
                Logger.Instance.LogInfo($"{id} just connected");
                playersNumInGame.Value++;
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if(IsServer )
            {
                Logger.Instance.LogInfo($"{id} just disconnected");
                playersNumInGame.Value--;
            }
        };
    }
}
