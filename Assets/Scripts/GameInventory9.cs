using UnityEngine;

public class GameInventory9 : MonoBehaviour
{
    public static GameInventory9 I { get; private set; }

    [System.Serializable]
    public class ItemDef
    {
        public string name;
        public Sprite icon;
        public GameObject worldPrefab; // optional, if later you want world visuals
        public int count = 3;
    }

    [Header("9 items in order (1..9)")]
    public ItemDef[] items = new ItemDef[9];

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public int GetCount(int itemIndex0) => items[itemIndex0].count;

    public bool TryTake(int itemIndex0)
    {
        if (items[itemIndex0].count <= 0) return false;
        items[itemIndex0].count--;
        return true;
    }

    public void AddBack(int itemIndex0, int amount = 1)
    {
        items[itemIndex0].count += amount;
    }
}