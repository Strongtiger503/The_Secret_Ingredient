using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Core Stats")]
    public string enemyName = "Basic Grunt";
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthBar;

    [Header("Hit Feedback & Knockback")]
    public float knockbackForce = 7f;
    public float knockbackDrag = 10f; // High drag stops them from sliding forever
    public Color damageFlashColor = Color.red;

    [Header("Loot Drop")]
    public GameObject collectablePrefab; // Drag your Skull Prefab here in the Inspector!

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Using Unity 6 linearDamping instead of drag
        rb.linearDamping = knockbackDrag;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    // We added attackerPosition so the enemy knows which way to fly back
    public void TakeDamage(float damageAmount, Vector3 attackerPosition)
    {
        currentHealth -= damageAmount;

        // Critical log for debugging hits
        Debug.Log($"[{enemyName}] took {damageAmount} damage. Health remaining: {currentHealth}");

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // Apply Knockback Physics
        Vector2 knockbackDirection = (transform.position - attackerPosition).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Flash Red
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        Debug.Log($"[{enemyName}] has been defeated.");

        // Drop the collectable before disappearing
        if (collectablePrefab != null)
        {
            Instantiate(collectablePrefab, transform.position, Quaternion.identity);
            Debug.Log($"Loot dropped at {transform.position}");
        }
        else
        {
            Debug.LogWarning("No collectable prefab assigned to enemy!");
        }

        gameObject.SetActive(false);
    }
}