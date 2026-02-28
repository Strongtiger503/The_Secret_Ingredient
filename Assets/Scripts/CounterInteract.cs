using TMPro;
using UnityEngine;

public class CounterInteract : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TMP_Text promptText;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool inRange;
    private PlayerCarry2D playerCarry;
    private PlayerInventory2 PlayerInventory2;

    private void Awake()
    {
        if (promptBox != null)
            promptBox.SetActive(false);
    }

    private void Update()
    {
        if (!inRange || playerCarry == null || PlayerInventory2 == null) return;

        UpdatePrompt();

        if (Input.GetKeyDown(interactKey))
        {
            if (!playerCarry.IsHolding)
            {
                // Must have item in selected slot to pick
                if (!PlayerInventory2.CanTakeSelected())
                    return;

                // Spawn carried item based on selected slot
                bool spawned = playerCarry.PickUpFromSlot(PlayerInventory2.SelectedSlot);
                if (spawned)
                {
                    PlayerInventory2.TryTakeSelected(); // subtract 1
                }
            }
            else
            {
                // Deposit back to counter -> refund the slot you were holding
                int slot = playerCarry.CarriedSlot;
                playerCarry.DepositAndDelete();
                PlayerInventory2.RefundToSlot(slot); // add 1 back
            }

            UpdatePrompt();
        }
    }

    private void UpdatePrompt()
    {
        if (promptBox == null || promptText == null) return;

        promptBox.SetActive(true);

        if (playerCarry.IsHolding)
        {
            promptText.text = "Press E to drop";
        }
        else
        {
            // Optional: show out-of-stock feedback
            promptText.text = PlayerInventory2.CanTakeSelected()
                ? "Press E to pickup"
                : "Out of stock";
        }
    }

    private void HidePrompt()
    {
        if (promptBox != null)
            promptBox.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerCarry = other.GetComponent<PlayerCarry2D>();
        if (playerCarry == null)
        {
            Debug.LogError("Player is missing PlayerCarry2D script!");
            return;
        }

        PlayerInventory2 = other.GetComponent<PlayerInventory2>();
        if (PlayerInventory2 == null)
        {
            Debug.LogError("Player is missing PlayerInventory2 script!");
            return;
        }

        inRange = true;
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        playerCarry = null;
        PlayerInventory2 = null;
        HidePrompt();
    }
}