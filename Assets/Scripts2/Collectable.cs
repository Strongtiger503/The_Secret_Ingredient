using UnityEngine;

public class Collectable : MonoBehaviour
{
    public enum LootType { Green, Blue, Red }

    [Header("Loot Settings")]
    public LootType lootType;
    public int itemValue = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Requires the Player object to have the Tag "Player"
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            if (inventory != null)
            {
                inventory.AddCollectable(lootType, itemValue);
                // Destroy removes it from the scene entirely once picked up
                Destroy(gameObject);
            }
        }
    }
}