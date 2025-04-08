using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player;              
    public float followDelay = 20f;     
    public float followSpeed = 2f;      
    public float followDistance = 5f;      

    private float timer = 0f;            
    private bool canFollow = false;      

    void Update()
    {
        if (!canFollow)
        {
            timer += Time.deltaTime;
            if (timer >= followDelay)
            {
                canFollow = true; 
            }
        }

        if (canFollow)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(player.position.x - followDistance, currentPosition.y, currentPosition.z);

            transform.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}
