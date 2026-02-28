using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Feedback Settings")]
    [Tooltip("How hard the player gets bumped backward when hit by normal attacks.")]
    public float knockbackForce = 10f;

    [Header("UI Elements")]
    public Slider healthBar;
    public GameObject gameOverPanel;

    // We add a reference to your movement script
    private playerMovement2 movementScript;

    void Start()
    {
        currentHealth = maxHealth;

        // Grab the movement script so we can disable it later
        movementScript = GetComponent<playerMovement2>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Debug.Log($"System: PlayerHealth initialized.");
    }

    // We added an optional parameter 'customKnockback'. If left empty, it uses the default -1f.
    public void TakeDamage(float amount, Vector2 attackerPosition, float customKnockback = -1f)
    {
        currentHealth -= amount;
        Debug.Log($"Combat: Player took {amount} damage! Current Health: {currentHealth}");

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // Trigger the soft bump (or massive explosion blast) on the movement script
        if (movementScript != null && currentHealth > 0)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - attackerPosition).normalized;

            // If customKnockback is passed in, use it. Otherwise, use the default knockbackForce.
            float appliedKnockback = customKnockback < 0f ? knockbackForce : customKnockback;

            movementScript.ApplyKnockback(knockbackDir, appliedKnockback);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void InstantKill()
    {
        TakeDamage(maxHealth * 10, transform.position);
    }

    void Die()
    {
        Debug.Log("Game Over: Player has died!");

        // 1. Show the Game Over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 2. Pause the game world
        Time.timeScale = 0f;

        // 3. Disable WASD and Mouse Clicking!
        if (movementScript != null)
        {
            movementScript.enabled = false;
            Debug.Log("System: Player controls disabled.");
        }

        // 4. Hide the player visually
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void RestartScene()
    {
        // Must reset time back to normal before loading, otherwise the new scene will be frozen too!
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("System: Scene Restarted.");
    }
}