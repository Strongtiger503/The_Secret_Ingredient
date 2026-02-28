using UnityEngine;
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // This creates a dropdown in the Inspector to choose the slime type
    public enum SlimeType { RedCreeper, BlueJumper, GreenTank }

    [Header("Identity")]
    public SlimeType slimeType;

    [Header("Base Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Swarm Behavior (Anti-Clumping)")]
    public float separationRadius = 1.5f;
    [Tooltip("How strongly the slime pushes away from other slimes.")]
    public float separationWeight = 1.5f;

    [Header("Melee Combat (Blue & Green)")]
    public float attackDamage = 15f;
    public float attackCooldown = 1f;
    private float nextAttackTime;

    [Header("Red Slime Specifics (Creeper)")]
    public float triggerRadius = 2.0f; // Distance where the fuse starts
    public float cancelRadius = 3.5f;  // Distance where the fuse aborts if player escapes
    public float explosionRadius = 4.5f; // Maximum distance the blast can reach
    public float maxExplosionDamage = 100f; // Damage if you are touching it
    public float explosionKnockback = 45f; // Massive knockback force
    public float fuseTime = 1.2f; // Time to escape before boom

    private bool isExploding = false; // Tracks if the fuse is lit
    private SpriteRenderer sr;
    private Vector3 originalScale;

    [Header("Blue Slime Specifics (Randomized)")]
    public float jumpForce = 8f;
    public float minJumpCooldown = 2f;
    public float maxJumpCooldown = 4.5f;
    [Tooltip("How erratic the jump direction is (0 = straight at player, 180 = completely random)")]
    [Range(0f, 180f)] public float jumpAngleVariance = 45f;

    private float nextJumpTime; // Tracks when the blue slime can jump again

    private Transform playerTarget;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Top-down game, no gravity
        rb.freezeRotation = true;

        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // Safely locate the player using the Tag system
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError("System: EnemyAI could not find an object tagged 'Player'!");
        }

        // Initialize the first random jump timer for Blue Slimes
        if (slimeType == SlimeType.BlueJumper)
        {
            nextJumpTime = Time.time + Random.Range(minJumpCooldown, maxJumpCooldown);
        }

        // Critical Task: Log successful spawn and intent
        Debug.Log($"EnemyAI: {slimeType} spawned and is hunting the Player.");
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

        // If the red slime is actively exploding, we skip movement so it stays rooted in place
        if (!isExploding)
        {
            // --- Anti-Clumping / Separation Logic ---
            Vector2 separationForce = Vector2.zero;
            int neighborCount = 0;

            // Find all colliders within the separation radius
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);

            foreach (Collider2D neighbor in neighbors)
            {
                // Only push away from other objects that have the EnemyAI script
                if (neighbor.gameObject != gameObject && neighbor.GetComponent<EnemyAI>() != null)
                {
                    Vector2 awayFromNeighbor = (Vector2)transform.position - (Vector2)neighbor.transform.position;

                    // Add force inversely proportional to distance (closer = stronger push)
                    if (awayFromNeighbor.magnitude > 0)
                    {
                        separationForce += awayFromNeighbor.normalized / awayFromNeighbor.magnitude;
                        neighborCount++;
                    }
                }
            }

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
            }

            // Blend the direction toward the player with the urge to stay away from other slimes
            Vector2 finalMoveDirection = (directionToPlayer + (separationForce * separationWeight)).normalized;

            // Base Movement: Apply the blended direction
            rb.linearVelocity = finalMoveDirection * moveSpeed;
        }
        else
        {
            // Lock movement to zero while hissing/exploding
            rb.linearVelocity = Vector2.zero;
        }

        // Execute specific behaviors based on the chosen Slime Type
        switch (slimeType)
        {
            case SlimeType.RedCreeper:
                // Start the explosion sequence if close enough and not already exploding
                if (!isExploding && distanceToPlayer <= triggerRadius)
                {
                    StartCoroutine(ExplosionRoutine());
                }
                break;

            case SlimeType.BlueJumper:
                // Jump if the timer is up and we aren't too close to the player
                if (!isExploding && Time.time >= nextJumpTime && distanceToPlayer > 1.5f)
                {
                    // Create a random angle offset based on your variance setting
                    float randomOffset = Random.Range(-jumpAngleVariance, jumpAngleVariance);

                    // Convert the player direction to an angle, add the random offset, and convert back to a vector
                    float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                    float randomAngle = baseAngle + randomOffset;

                    Vector2 randomizedJumpDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));

                    PerformJump(randomizedJumpDir);
                }
                break;

            case SlimeType.GreenTank:
                // Green slimes just rely on base movement and heavy collision damage.
                break;
        }
    }

    IEnumerator ExplosionRoutine()
    {
        isExploding = true;
        Debug.Log("Combat: Red Slime triggered! Fuse started.");

        Color originalColor = sr != null ? sr.color : Color.red;
        float timer = 0f;

        // Visually swell up and flash white during the fuse time
        while (timer < fuseTime)
        {
            // CREEPER MECHANIC: Check if the player got far enough away to cancel the fuse
            if (playerTarget != null)
            {
                float currentDist = Vector2.Distance(transform.position, playerTarget.position);
                if (currentDist > cancelRadius)
                {
                    Debug.Log("Combat: Player escaped! Red Slime canceled detonation.");
                    isExploding = false;

                    // Deflate and reset color immediately
                    if (sr != null) sr.color = originalColor;
                    transform.localScale = originalScale;

                    yield break; // Exit the coroutine completely so it doesn't explode
                }
            }

            timer += Time.deltaTime;

            if (sr != null)
            {
                // Ping-pongs the color rapidly between original and white
                float lerp = Mathf.PingPong(timer * 8f, 1f);
                sr.color = Color.Lerp(originalColor, Color.white, lerp);

                // Swell the slime up by 30% smoothly
                transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, timer / fuseTime);
            }
            yield return null;
        }

        // BOOM! Evaluate damage based on the player's final distance
        if (playerTarget != null)
        {
            float finalDistanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (finalDistanceToPlayer <= explosionRadius)
            {
                // Calculate percentage (0.0 to 1.0). If touching (0 distance), percentage is 1 (100% damage).
                float damagePercent = 1f - (finalDistanceToPlayer / explosionRadius);
                float finalDamage = maxExplosionDamage * damagePercent;

                if (playerHealth != null)
                {
                    // Pass the damage, position, AND the custom massive knockback force
                    playerHealth.TakeDamage(finalDamage, transform.position, explosionKnockback);
                }
                Debug.Log($"Combat: Red Slime exploded! Dealt {Mathf.Round(finalDamage)} scaled damage.");
            }
        }

        // Destroy the slime immediately after exploding
        Destroy(gameObject);
    }

    void PerformJump(Vector2 jumpDirection)
    {
        Debug.Log("Combat: Blue Slime executed a randomized dash/jump!");

        // Apply a sudden burst of speed in the randomized direction
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        // Reset the timer with a brand new random interval
        nextJumpTime = Time.time + Random.Range(minJumpCooldown, maxJumpCooldown);
    }

    // Handle physical attacks when touching the player
    void OnCollisionStay2D(Collision2D collision)
    {
        // Red slimes don't use melee attacks anymore, they only explode
        if (slimeType == SlimeType.RedCreeper) return;

        if (collision.gameObject.CompareTag("Player") && Time.time >= nextAttackTime)
        {
            if (playerHealth != null)
            {
                // Pass the position, but leave custom knockback empty so it defaults to the soft bump
                playerHealth.TakeDamage(attackDamage, transform.position);
                Debug.Log($"Combat: {slimeType} struck the player for {attackDamage} damage.");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
}