using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float verticalVelocity = 0f;
    public float gravity = 10f;
    public float lookupClamp = 80f;
    public float mousesensitivity = 2f;
    public float dodgeCooldown = 1.5f;
    public float dodgeDuration = 0.5f;
    public float dodgeSpeedMultiplier = 2f;
    public float teleportDistance = 5f;
    public bool isInvincible { get; private set; } = false;
    public GameObject dodgeEffectPrefab;
    public GameObject teleportEndEffectPrefab; 
    [SerializeField]
    private Transform mainCamera;
    private CharacterController characterController;
    private Animator anim;
    private Vector3 movementDirection;
    private float rotationX = 0f;
    private float lastDodgeTime = -10f;
    private float dodgeEndTime = 0f;
    private bool isDodging = false;
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
        HandleDodge();
        if (characterController.isGrounded)
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
        float currentSpeed = isDodging ? moveSpeed * dodgeSpeedMultiplier : moveSpeed;
        horizontalMovement *= currentSpeed;
        bool running = (horizontalMovement.magnitude > 0.1f);
        anim.SetBool("isRunning", running);
        movementDirection = horizontalMovement;
        movementDirection.y = verticalVelocity;
        characterController.Move(movementDirection * Time.deltaTime);
    }
    private void HandleDodge()
    {
        if (Time.time > dodgeEndTime)
        {
            isInvincible = false;
            isDodging = false;
            anim.SetBool("Dodge", false);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDodge())
        {
            Vector3 startPosition = transform.position;

            Vector3 teleportDirection;
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
            {
                teleportDirection = transform.right * horizontalInput + transform.forward * verticalInput;
                teleportDirection.Normalize();
            }
            else
            {
                teleportDirection = transform.forward;
            }

            Vector3 destination = transform.position + teleportDirection * teleportDistance;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(destination.x, destination.y + 1f, destination.z), Vector3.down, out hit, 3f))
            {
                destination.y = hit.point.y;
            }

            RaycastHit wallHit;
            if (Physics.Raycast(startPosition, teleportDirection, out wallHit, teleportDistance))
            {
                destination = wallHit.point - (teleportDirection * 0.5f);
            }

            if (dodgeEffectPrefab != null)
            {
                Instantiate(dodgeEffectPrefab, transform.position, Quaternion.identity);
            }

            characterController.enabled = false;
            transform.position = destination;
            characterController.enabled = true;

            if (teleportEndEffectPrefab != null)
            {
                Instantiate(teleportEndEffectPrefab, transform.position, Quaternion.identity);
            }

            lastDodgeTime = Time.time;
            dodgeEndTime = Time.time + dodgeDuration;
            isInvincible = true;
            isDodging = true;

            if (anim != null)
            {
                anim.SetBool("Dodge", true);
            }

            Debug.Log("Player dodged!");
        }
    }
    private bool CanDodge()
    {
        return Time.time > lastDodgeTime + dodgeCooldown;
    }
}