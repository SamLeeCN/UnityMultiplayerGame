using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Button startServerBtn;
    [SerializeField] private Button startHostBtn;
    [SerializeField] private Button startClientBtn;
    [SerializeField] private Button instantiateSphereBtn;
    [SerializeField] private TextMeshProUGUI playerNumberTxt;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private bool hasServerStarted = false;

    private void Awake()
    {
        Cursor.visible = true;
        hasServerStarted = false;
    }

    private void Update()
    {
        playerNumberTxt.text = "Player Number:" + PlayersManager.Instance.PlayersNumInGame;
    }
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted=true;
        };

        startServerBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Logger.Instance.LogInfo("Server started");
            }
            else
            {
                Logger.Instance.LogInfo("Server starting failed");
            }
        });

        startHostBtn.onClick.AddListener(async () =>
        {
            if(RelayManager.Instance.IsRelayEnabled)
                await RelayManager.Instance.SetUpRelay();

            if (NetworkManager.Singleton.StartHost())
            {
                Logger.Instance.LogInfo("Host started");
            }
            else
            {
                Logger.Instance.LogInfo("Host starting failed");
            }

        });

        startClientBtn.onClick.AddListener(async () =>
        {
            
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInputField.text))
                await RelayManager.Instance.JoinRelay(joinCodeInputField.text);

            if (NetworkManager.Singleton.StartClient())
            {
                Logger.Instance.LogInfo("Client started");
            }
            else
            {
                Logger.Instance.LogInfo("Client starting failed");
            }

        });


        instantiateSphereBtn.onClick.AddListener(() =>
        {
            if(!hasServerStarted)
            {
                Logger.Instance.LogInfo("Server hasn't been started");
                return;
            }

            SphereSpwaner.Instance.InstantiateSpheres();
        });
    }
}
