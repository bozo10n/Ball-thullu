using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // variables for movement
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float verticalVelocity = 0f;
    public float gravity = 10f;
    private Vector3 movementDirection;

    public float lookupClamp = 80f; // maximum angle for looking down and up 
    public float mousesensitivity = 2f; // self explanatory idiot 
    private float rotationX = 0f;

    [SerializeField]
    private Transform mainCamera; // referene to main camera
    private CharacterController characterController; // reference to character controller
    

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouse();

        HandleMovement();
    }

    private void HandleMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mousesensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity;

        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -lookupClamp, lookupClamp); // setting the min and max value for rotationX 

        mainCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        transform.Rotate(Vector3.up, mouseX);
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * 2f);
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

        movementDirection = horizontalMovement;
        movementDirection.y = verticalVelocity;

        characterController.Move(movementDirection * Time.deltaTime);
    }

}