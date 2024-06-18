using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.Playables;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerControllerAuthoritive : NetworkBehaviour
{
    public enum PlayerAnimationState
    {
        Idle,
        RunForward,
        RunBackward,
        Punch
    }

    [SerializeField] private float walkSpeed = 0.01f;
    [SerializeField] private float rotateSpeed = 0.1f;
    [SerializeField] private Vector2 defaultPosRange = new Vector2(-4, 4);
    [SerializeField] private NetworkVariable<PlayerAnimationState> networkPlayerAnimationState = new NetworkVariable<PlayerAnimationState>();
    [SerializeField] public NetworkVariable<float> networkPlayerHealth = new NetworkVariable<float>(2000);

    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    [SerializeField] private float minPunchDistance;
    [SerializeField] private int punchDamage=1;

    [Header("Reference")]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if(IsClient && IsOwner)
        {
            transform.position = new Vector3
            (Random.Range(defaultPosRange.x, defaultPosRange.y), 0, Random.Range(defaultPosRange.x, defaultPosRange.y));
            PlayerCameraFollow.Instance.FollowPlayer(transform);
        }
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        if(IsClient&&IsOwner)
        {
            ClientInput();
        }
        ClientAnimation();
    }

    private void FixedUpdate()
    {
        
    }
    void AttackHit()//called in animation
    {
        if (IsClient && IsOwner)
        {
            CheckPunch(leftHand.transform, Vector3.left);
            CheckPunch(rightHand.transform, Vector3.right);
        }
    }
    void ClientAnimation()
    {
        
        if (networkPlayerAnimationState.Value == PlayerAnimationState.RunForward)
        {
            animator.SetFloat("Speed", 1); 
        }
        else if(networkPlayerAnimationState.Value == PlayerAnimationState.RunBackward)
        {
            animator.SetFloat("Speed", -1);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        if (networkPlayerAnimationState.Value==PlayerAnimationState.Punch)
        {
            animator.SetTrigger("Attack");
        }
    }

    void ClientInput()
    {
        Vector3 newRotateMoveInput = new Vector3 (0,0,0);
        Vector3 inputPosRelativeMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 inputPosForwardDir = transform.TransformDirection(transform.forward);
        Vector3 inputPosRightDir = transform.TransformDirection(transform.right);
        Vector3 newPosMoveInput = Input.GetAxis("Horizontal") * inputPosRightDir + Input.GetAxis("Vertical") * inputPosForwardDir;

        //Client is responsible for the transform of itself.
        transform.position += newPosMoveInput * walkSpeed;
        transform.Rotate(newRotateMoveInput * rotateSpeed, Space.World);
        
        //Player Animation State
        if (inputPosRelativeMove.z > 0.5)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.RunForward);
        }else if(inputPosRelativeMove.z < -0.5)
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.RunBackward);
        }
        else
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Idle);
        }

        if (Input.GetKey(KeyCode.E))
        {
            UpdatePlayerAnimationStateServerRpc(PlayerAnimationState.Punch);
        }
    }

    private void CheckPunch(Transform hand, Vector3 aimDirection)
    {
        RaycastHit hit;

        int layerMask = LayerMask.GetMask("Player");

        if (Physics.Raycast(hand.position, hand.transform.TransformDirection(aimDirection), out hit, minPunchDistance, layerMask))
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.yellow);

            var playerHit = hit.transform.GetComponent<NetworkObject>();
            if (playerHit != null)
            {
                UpdateHealthServerRpc(punchDamage, playerHit.OwnerClientId);
            }
        }
        else
        {
            Debug.DrawRay(hand.position, hand.transform.TransformDirection(aimDirection) * minPunchDistance, Color.red);
        }
    }
    public void TakeDamage(int damageValue)
    {
        Logger.Instance.LogInfo($"got punch: {damageValue}");
    }

    [ServerRpc]
    public void UpdateHealthServerRpc(int damageValue,ulong clientIdToHit)
    {
        
        PlayerControllerAuthoritive clientToDamage = NetworkManager.Singleton.ConnectedClients[clientIdToHit].
            PlayerObject.GetComponent<PlayerControllerAuthoritive>();

        if (clientToDamage != null && clientToDamage.networkPlayerHealth.Value > 0)
        {
            clientToDamage.networkPlayerHealth.Value -= damageValue;
        }
        
        NotifyHealthChangedClientRpc(damageValue, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientIdToHit }
            }
        });
    }

    [ClientRpc]
    public void NotifyHealthChangedClientRpc(int takeAwayPoint, ClientRpcParams clientRpcParams = default)
    {   
        if (IsOwner) return;
        
        TakeDamage(takeAwayPoint);
    }

    [ServerRpc]
    public void UpdatePlayerAnimationStateServerRpc(PlayerAnimationState playerAnimationState)
    {
        networkPlayerAnimationState.Value = playerAnimationState;
    }
}
