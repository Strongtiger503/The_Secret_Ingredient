using UnityEngine;
using TMPro; // Required to talk to the Text (TMP) UI elements

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Counts")]
    public int greenLoot = 0;
    public int blueLoot = 0;
    public int redLoot = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI greenLootText;
    public TextMeshProUGUI blueLootText;
    public TextMeshProUGUI redLootText;

    void Start()
    {
        // Set the UI to 0 right when the game starts
        UpdateUI();
    }

    public void AddCollectable(Collectable.LootType type, int amount)
    {
        // Add to the correct pile based on the color
        switch (type)
        {
            case Collectable.LootType.Green:
                greenLoot += amount;
                break;
            case Collectable.LootType.Blue:
                blueLoot += amount;
                break;
            case Collectable.LootType.Red:
                redLoot += amount;
                break;
        }

        // Critical Task: Log the updated inventory counts
        Debug.Log($"COLLECTED: Picked up {amount} {type} loot! Totals - G:{greenLoot} | B:{blueLoot} | R:{redLoot}");

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Check if the UI elements are assigned before trying to change their text
        if (greenLootText != null) greenLootText.text = greenLoot.ToString();
        if (blueLootText != null) blueLootText.text = blueLoot.ToString();
        if (redLootText != null) redLootText.text = redLoot.ToString();
    }
}