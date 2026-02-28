using UnityEngine;

public class MachineZoneDeposit : MonoBehaviour
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
            if (!playerHotbar.HasAny()) return;
            playerHotbar.DepositAllAndClear();
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