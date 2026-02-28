using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    [Header("Combat Settings")]
    public float weaponDamage = 25f;
    public float hitCooldown = 0.15f;

    private float nextHitTime;
    private playerMovement2 playerMovement;

    void Awake()
    {
        // Grab the player movement script from the parent object hierarchy
        playerMovement = GetComponentInParent<playerMovement2>();

        Debug.Log($"Weapon system online. Damage: {weaponDamage} | Cooldown: {hitCooldown}s.");

        if (playerMovement == null)
        {
            Debug.LogError("System: WeaponHit could not find playerMovement2 on a parent object!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // First, verify the player is actually performing an attack swing
        if (playerMovement != null && playerMovement.isSwinging)
        {
            // Then check if it's an enemy and if the cooldown has passed
            if (other.CompareTag("Enemy") && Time.time >= nextHitTime)
            {
                // Critical Task: Log the valid swing collision for debugging
                Debug.Log($"Combat: Active swing connected with {other.name}!");

                RegisterHit(other.gameObject);
                nextHitTime = Time.time + hitCooldown;
            }
        }
    }

    void RegisterHit(GameObject enemy)
    {
        // Looking for the EnemyHealth script
        EnemyHealth enemyStats = enemy.GetComponent<EnemyHealth>();

        if (enemyStats != null)
        {
            // Pass the weapon's position (transform.position) for the knockback math
            enemyStats.TakeDamage(weaponDamage, transform.position);
            Debug.Log($"SUCCESS: Weapon dealt {weaponDamage} damage to '{enemy.name}'!");
        }
        else
        {
            Debug.LogWarning($"Hit {enemy.name}, but it is missing the EnemyHealth script!");
        }
    }
}