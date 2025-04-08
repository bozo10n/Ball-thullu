using UnityEngine;

public class DamageOnCollision : MonoBehaviour
{
    public int damageAmount = 10;
    private bool hasDealtDamage = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasDealtDamage) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                hasDealtDamage = true;
            }
        }

        Destroy(gameObject, 0.1f);
    }
}