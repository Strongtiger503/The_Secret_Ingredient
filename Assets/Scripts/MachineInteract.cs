using TMPro;
using UnityEngine;

public class MachineInteract : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TMP_Text promptText;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool inRange;
    private PlayerCarry2D playerCarry;

    private void Awake()
    {
        if (promptBox != null)
            promptBox.SetActive(false);
    }

    private void Update()
    {
        if (!inRange || playerCarry == null) return;

        UpdatePrompt();

        if (Input.GetKeyDown(interactKey))
        {
            // Machine only accepts items (consumes them)
            if (playerCarry.IsHolding)
            {
                playerCarry.DepositAndDelete(); // item disappears
            }

            UpdatePrompt();
        }
    }

    private void UpdatePrompt()
    {
        if (promptBox == null || promptText == null) return;

        promptBox.SetActive(true);

        // Optional: show different text depending on if player has an item
        promptText.text = playerCarry.IsHolding ? "Press E to insert" : "Need item";
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

        inRange = true;
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        playerCarry = null;
        HidePrompt();
    }
}