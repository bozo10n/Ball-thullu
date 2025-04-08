using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions.Must;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float verticalVelocity = 0f;
    public float gravity = 10f;
    private Vector3 movementDirection;

    public float lookupClamp = 80f; 
    public float mousesensitivity = 2f; // self explanatory idiot 
    private float rotationX = 0f;

    [SerializeField]
    private Transform mainCamera; // referene to main camera
    private CharacterController characterController; // reference to character controller
    private Animator anim;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        HandleMovement();

        if(characterController.isGrounded)
        {
            anim.SetBool("isJumping", false);
        }    
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            
            verticalVelocity = -0.5f;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * 2f);
                anim.SetBool("isJumping", true);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 horizontalMovement = transform.right * horizontalInput + transform.forward * verticalInput;

        horizontalMovement *= moveSpeed;

        bool running = (horizontalMovement.magnitude > 0.1f);

        anim.SetBool("isRunning", running);

        movementDirection = horizontalMovement;
        movementDirection.y = verticalVelocity;

        characterController.Move(movementDirection * Time.deltaTime);
    }

}