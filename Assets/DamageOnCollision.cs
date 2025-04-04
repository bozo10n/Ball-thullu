using UnityEngine;

public class DamageOnCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float attackDamage = 10f;
    private bool hasDamaged = false;
    private void OnCollisionEnter(Collision collision)
    {
        if(!hasDamaged && collision.gameObject.CompareTag("Player"))
        {
            hasDamaged = true;

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>() ?? collision.gameObject.GetComponentInParent<PlayerHealth>() ?? collision.gameObject.GetComponentInChildren<PlayerHealth>();

            if (playerHealth == null)
            {
                Debug.LogError("player Health compontent is null");
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("Player is too far for damage");
            }

            Rigidbody pRb = collision.gameObject.GetComponent<Rigidbody>() ?? collision.gameObject.GetComponentInParent<Rigidbody>() ?? collision.gameObject.GetComponentInChildren<Rigidbody>();

            pRb.AddForce(collision.transform.position + Vector3.up * 100f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
