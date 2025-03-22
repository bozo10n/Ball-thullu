using UnityEngine;
using UnityEngine.UI; // Only needed if you're using UI elements to display health

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    // Optional UI reference
    public Slider healthBar;

    // Optional invincibility frames
    public float invincibilityTime = 0.5f;
    private float lastDamageTime = -10f;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damageAmount)
    {
        // Check for invincibility frames
        if (Time.time < lastDamageTime + invincibilityTime)
            return;

        lastDamageTime = Time.time;

        currentHealth -= damageAmount;
        UpdateHealthUI();

        // Play damage effects
        // Example: GetComponent<AudioSource>().Play();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        // Update UI if available
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        // Handle player death
        Debug.Log("Player has died!");

        // Example: Reload level, show game over screen, etc.
        // SceneManager.LoadScene("GameOver");

        // Or just reset health for testing
        // currentHealth = maxHealth;
        // UpdateHealthUI();
    }
}