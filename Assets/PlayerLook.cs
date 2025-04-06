using UnityEngine;

public class PlayerLook : MonoBehaviour
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
    private Animator anim;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMouse();
    }

    private void HandleMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mousesensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity;

        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -lookupClamp, lookupClamp);

        mainCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        transform.Rotate(Vector3.up, mouseX);
    }
}
