using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;               // Reference to the player transform
    public float followDelay = 20f;        // Time in seconds before following starts
    public float followSpeed = 2f;         // Speed at which the object follows
    public float followDistance = 5f;      // Desired distance on the x-axis

    private float timer = 0f;              // Timer to track time passed
    private bool canFollow = false;        // Flag to start following after delay

    void Update()
    {
        // Increment timer until followDelay is reached
        if (!canFollow)
        {
            timer += Time.deltaTime;
            if (timer >= followDelay)
            {
                canFollow = true;  // Start following after 20 seconds
            }
        }

        if (canFollow)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(player.position.x - followDistance, currentPosition.y, currentPosition.z);

            // Smoothly move towards the target position
            transform.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}
