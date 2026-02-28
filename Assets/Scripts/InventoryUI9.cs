using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI9 : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image icon;
        public TMP_Text label; // show "1\nx3" etc
    }

    [SerializeField] private SlotUI[] slotUIs = new SlotUI[9];
    [SerializeField] private PlayerHotbar3 player; // drag Player here

    private void Update()
    {
        var inv = GameInventory9.I;
        if (inv == null) return;

        for (int i = 0; i < 9; i++)
        {
            if (slotUIs[i].icon != null) slotUIs[i].icon.sprite = inv.items[i].icon;

            int count = inv.GetCount(i);
            bool selected = (player != null && player.SelectedItemIndex0 == i);

            if (slotUIs[i].label != null)
                slotUIs[i].label.text = $"{(selected ? "> " : "")}{i + 1}\nx{count}";
        }
    }
}