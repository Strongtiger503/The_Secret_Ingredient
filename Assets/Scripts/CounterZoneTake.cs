using UnityEngine;

public class CounterZoneTake : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject hotbarPanel; // drag HotbarPanel here

    private bool inRange;
    private PlayerHotbar3 playerHotbar;

    private void Update()
    {
        if (!inRange || playerHotbar == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            int itemIndex = playerHotbar.SelectedItemIndex0;

            if (!playerHotbar.HasSpace()) return;
            if (!GameInventory9.I.TryTake(itemIndex)) return;

            playerHotbar.AddToHotbar(itemIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerHotbar = other.GetComponent<PlayerHotbar3>();
        if (playerHotbar == null) { Debug.LogError("Player missing PlayerHotbar3"); return; }

        inRange = true;
        if (hotbarPanel != null) hotbarPanel.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        playerHotbar = null;
        if (hotbarPanel != null) hotbarPanel.SetActive(false);
    }
}