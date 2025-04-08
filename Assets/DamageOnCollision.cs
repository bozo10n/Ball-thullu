using UnityEngine;
public class DamageOnCollision : MonoBehaviour
{
    public float damageAmount = 10f;
    public bool destroyOnImpact = true;
    public GameObject impactEffectPrefab;
    public float lifespan = 3f;

    private void Start()
    {
        Destroy(gameObject, lifespan);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.isInvincible)
            {
                Debug.Log("Player dodged the attack!");
                if (impactEffectPrefab != null)
                {
                    Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
                return;
            }
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Player hit for {damageAmount} damage!");
            }
            else
            {
                Debug.LogWarning("Player hit but no PlayerHealth component found!");
            }
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            }
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}