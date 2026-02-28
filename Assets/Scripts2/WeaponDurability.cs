using UnityEngine;

public class WeaponDurability : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float health = 100f;
    public float damageTakenPerHit = 25f;
    public float hitCooldown = 0.5f;

    private float nextHitTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hitting an enemy and if the cooldown has passed
        if (other.CompareTag("Enemy") && Time.time >= nextHitTime)
        {
            DecreaseDurability();
            nextHitTime = Time.time + hitCooldown;
        }
    }

    void DecreaseDurability()
    {
        health -= damageTakenPerHit;
        Debug.Log($"Weapon hit an enemy! Durability left: {health}");

        if (health <= 0)
        {
            Debug.Log("Weapon has broken and will be removed.");
            gameObject.SetActive(false); // Disappears from the player's hand
        }
    }
}