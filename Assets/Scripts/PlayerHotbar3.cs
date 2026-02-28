using UnityEngine;

public class PlayerHotbar3 : MonoBehaviour
{
    // each slot stores itemIndex (0..8), or -1 empty
    public int[] slots = new int[3] { -1, -1, -1 };

    public int SelectedItemIndex0 { get; private set; } = 0; // default = item 1

    private void Update()
    {
        // Select item type using 1..9 keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectedItemIndex0 = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectedItemIndex0 = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectedItemIndex0 = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectedItemIndex0 = 3;
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectedItemIndex0 = 4;
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectedItemIndex0 = 5;
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectedItemIndex0 = 6;
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectedItemIndex0 = 7;
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectedItemIndex0 = 8;
    }

    public bool HasSpace()
    {
        for (int i = 0; i < 3; i++) if (slots[i] == -1) return true;
        return false;
    }

    public bool AddToHotbar(int itemIndex0)
    {
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] == -1)
            {
                slots[i] = itemIndex0;
                return true;
            }
        }
        return false;
    }

    public bool HasAny()
    {
        for (int i = 0; i < 3; i++) if (slots[i] != -1) return true;
        return false;
    }

    public int DepositAllAndClear()
    {
        int deposited = 0;
        for (int i = 0; i < 3; i++)
        {
            if (slots[i] != -1)
            {
                deposited++;
                slots[i] = -1;
            }
        }
        return deposited;
    }
}