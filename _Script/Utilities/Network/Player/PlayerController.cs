using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//*****************************************
//创建人： SamLee 
//功能说明：
//***************************************** 
public class PlayerController : NetworkBehaviour
{
    public enum PlayerAnimationState
    {
        Idle,
        RunForward,
        RunBackward
    }

    [SerializeField] private float walkSpeed = 0.01f;
    [SerializeField] private float rotateSpeed = 0.1f;
    [SerializeField] private Vector2 defaultPosRange = new Vector2(-4, 4);
    [SerializeField] private NetworkVariable<Vector3> networkPosMove = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<Vector3> networkRotateMove = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<PlayerAnimationState> networkPlayerAnimationState = new NetworkVariable<PlayerAnimationState>();


    //client caching
    private Vector3 oldPosMoveInput;
    private Vector3 oldRotateMoveInput;
    [Header("Reference")]
    [SerializeField] private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        transform.position = new Vector3
            (Random.Range(defaultPosRange.x, defaultPosRange.y), 0, Random.Range(defaultPosRange.x, defaultPosRange.y));
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        if(IsClient&&IsOwner)
        {
            ClientInput();
        }
        //update move and rotate
        ClientMoveAndRotate();
        //update animation
        ClientAnimation();
    } 

    void ClientMoveAndRotate()
    {
        if (networkPosMove.Value != Vector3.zero)
        {
            //characterController.SimpleMove(networkPosMove.Value);
            transform.position += networkPosMove.Value*walkSpeed;
        }

        if(networkRotateMove.Value != Vector3.zero)
        {
            transform.Rotate(networkRotateMove.Value*rotateSpeed);
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
    }

    void ClientInput()
    {
        Vector3 newRotateMoveInput = new Vector3 (0,0,0);
        Vector3 inputPosRelativeMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 inputPosForwardDir = transform.TransformDirection(transform.forward);
        Vector3 inputPosRightDir = transform.TransformDirection(transform.right);
        Vector3 newPosMoveInput = Input.GetAxis("Horizontal") * inputPosRightDir + Input.GetAxis("Vertical") * inputPosForwardDir;
        

        //Movement of Pos and Rotate
        if(oldPosMoveInput!=newPosMoveInput||oldRotateMoveInput!=newRotateMoveInput) 
        {//Not being call every frame in order to reduce the calculation of server
            oldPosMoveInput = newPosMoveInput;
            oldRotateMoveInput = newRotateMoveInput;
            //Update to the server
            UpdateClientMoveToServerRpc(newPosMoveInput,newRotateMoveInput);
        }

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

    }

    [ServerRpc]
    public void UpdateClientMoveToServerRpc(Vector3 posMove,Vector3 rotateMove)
    {
        networkPosMove.Value = posMove;
        networkRotateMove.Value = rotateMove;
    }

    [ServerRpc]
    public void UpdatePlayerAnimationStateServerRpc(PlayerAnimationState playerAnimationState)
    {
        networkPlayerAnimationState.Value = playerAnimationState;
    }
}
