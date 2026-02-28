using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class playerMovement2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float movementSmoothing = 0.05f;

    [Header("Knockback Physics")]
    [Tooltip("How fast the pushback force wears off. Higher = shorter push.")]
    public float knockbackDecay = 10f;

    [Header("Scene Specific (Combat Scene)")]
    public bool canJump = false;
    public float jumpForce = 12f;

    [Header("Aim & Weapon Settings")]
    public Camera mainCamera; // Required to track the mouse
    public Transform weaponPivot;
    public float swingDuration = 0.15f;

    // Made public so WeaponHit.cs can check if we are attacking
    public bool isSwinging = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private Vector2 knockbackVelocity; // Stores the active pushback force

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Auto-assign the camera if it was left empty
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log("System: Main Camera auto-assigned for mouse tracking.");
        }

        Debug.Log($"System: playerMovement2 active on {gameObject.name}");
    }

    void Update()
    {
        // 1. Movement Input
        Vector2 rawInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) rawInput.y += 1;
            if (Keyboard.current.sKey.isPressed) rawInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) rawInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) rawInput.x += 1;
        }
        moveInput = rawInput.normalized;

        // 2. Aim at Mouse (Only when not actively swinging)
        if (!isSwinging && mainCamera != null && weaponPivot != null && Mouse.current != null)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

            // Calculate the direction from the pivot to the mouse
            Vector2 lookDirection = mouseWorldPosition - (Vector2)weaponPivot.position;

            // Calculate the angle and apply it to the pivot
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 3. Jump/Dash (Space Bar)
        if (canJump && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(moveInput * jumpForce, ForceMode2D.Impulse);
            Debug.Log($"Combat Ability: Dash Performed in direction {moveInput}");
        }

        // 4. Weapon Swing (Left Click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isSwinging && weaponPivot != null)
        {
            StartCoroutine(PerformSwing());
        }
    }

    IEnumerator PerformSwing()
    {
        isSwinging = true;
        // Lock in the starting rotation (where the mouse was aiming)
        Quaternion startRot = weaponPivot.rotation;

        // Calculate the end rotation (150 degrees from the start)
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, -170f);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / swingDuration;
            // Smoothly rotate between the start and end point
            weaponPivot.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        // Reset the swinging state. The next Update() frame will instantly snap it back to the mouse position.
        isSwinging = false;
        Debug.Log("Combat: Weapon swing animation complete.");
    }

    void FixedUpdate()
    {
        // Decay the knockback force smoothly back to zero over time
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);

        // Blend the player's intentional movement with the external pushback force
        Vector2 playerIntentVelocity = moveInput * moveSpeed;
        Vector2 targetVelocity = playerIntentVelocity + knockbackVelocity;

        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
    }

    // Public method so PlayerHealth can trigger the physical push
    public void ApplyKnockback(Vector2 direction, float force)
    {
        // Instantly spike the knockback velocity in the opposite direction
        knockbackVelocity = direction * force;
        Debug.Log($"System: Player pushed back with force {force}.");
    }
}