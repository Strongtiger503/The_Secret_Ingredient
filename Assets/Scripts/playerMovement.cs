using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Useful if you want to check scene names

[RequireComponent(typeof(Rigidbody2D))]
public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float movementSmoothing = 0.05f;

    [Header("Scene Specific Abilities")]
    public bool canJump = false; // Toggle this ON in the Inspector for the combat scene
    public float jumpForce = 10f;
    public float jumpCooldown = 0.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private float lastJumpTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        Debug.Log($"System: playerMovement initialized on {gameObject.name}. Can Jump: {canJump}");
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // 1. Standard Movement Input
            Vector2 rawInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) rawInput.y += 1;
            if (Keyboard.current.sKey.isPressed) rawInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) rawInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) rawInput.x += 1;

            moveInput = rawInput.normalized;

            // 2. Scene-Specific Jump Logic
            // Checks if ability is enabled, Space is pressed, and cooldown is over
            if (canJump && Keyboard.current.spaceKey.wasPressedThisFrame && Time.time > lastJumpTime + jumpCooldown)
            {
                HandleJump();
            }
        }
    }

    void HandleJump()
    {
        // Direction is based on where you are currently walking
        // If standing still, we jump in the 'Up' direction by default
        Vector2 jumpDir = moveInput != Vector2.zero ? moveInput : Vector2.up;

        // Applying an instantaneous burst of force
        rb.AddForce(jumpDir * jumpForce, ForceMode2D.Impulse);

        lastJumpTime = Time.time;

        Debug.Log($"Jumping forward in direction: {jumpDir} with force: {jumpForce}");
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Using linearVelocity (Unity 6 specific)
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
    }
}