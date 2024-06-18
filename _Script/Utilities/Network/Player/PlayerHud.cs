using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
public class PlayerHud : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();

    private bool isOverlaySet = false;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            playerName.Value = $"Player {OwnerClientId}";
        }
    }
    
    public void SetOverLay()
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = playerName.Value;
    }

    private void Update()
    {
        if(!isOverlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverLay();
            isOverlaySet = true;
        }
    }
}
