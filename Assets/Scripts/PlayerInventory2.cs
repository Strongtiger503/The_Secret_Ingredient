using TMPro;
using UnityEngine;

public class PlayerInventory2 : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text invText;

    [Header("Counts (starting inventory)")]
    [SerializeField] private int slot1 = 3;
    [SerializeField] private int slot2 = 3;
    [SerializeField] private int slot3 = 3;

    public int SelectedSlot { get; private set; } = 1;

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SelectedSlot = 1; RefreshUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SelectedSlot = 2; RefreshUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SelectedSlot = 3; RefreshUI(); }
    }

    public bool HasAnyItem()
    {
        return slot1 + slot2 + slot3 > 0;
    }

    public bool CanTakeSelected()
    {
        return GetCount(SelectedSlot) > 0;
    }

    public bool TryTakeSelected()
    {
        int c = GetCount(SelectedSlot);
        if (c <= 0) return false;
        SetCount(SelectedSlot, c - 1);
        RefreshUI();
        return true;
    }

    public void RefundToSlot(int slot)
    {
        SetCount(slot, GetCount(slot) + 1);
        RefreshUI();
    }

    public int GetCount(int slot)
    {
        return slot switch
        {
            1 => slot1,
            2 => slot2,
            3 => slot3,
            _ => 0
        };
    }

    private void SetCount(int slot, int value)
    {
        value = Mathf.Max(0, value);
        if (slot == 1) slot1 = value;
        else if (slot == 2) slot2 = value;
        else if (slot == 3) slot3 = value;
    }

    public void RefreshUI()
    {
        if (invText == null) return;

        // Simple highlight: add ">" on selected
        invText.text =
            $"{(SelectedSlot == 1 ? "> " : "  ")}1) Item 1: {slot1}\n" +
            $"{(SelectedSlot == 2 ? "> " : "  ")}2) Item 2: {slot2}\n" +
            $"{(SelectedSlot == 3 ? "> " : "  ")}3) Item 3: {slot3}";
    }
}