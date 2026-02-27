using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

[RequireComponent(typeof(Rigidbody2D))]
public class playerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float movementSmoothing = 0.05f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Ensure the player doesn't spin when hitting walls
        rb.freezeRotation = true;
    }

    void Update()
    {
        // New Input System way to read WASD/Arrows (Vector2)
        // This polls the keyboard directly without needing an Input Action Asset
        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            Vector2 rawInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) rawInput.y += 1;
            if (Keyboard.current.sKey.isPressed) rawInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) rawInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) rawInput.x += 1;

            moveInput = rawInput.normalized;
        }

        // Debugging logs
        if (moveInput != Vector2.zero)
        {
            Debug.Log($"Moving: {moveInput}");
        }
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
    }
}