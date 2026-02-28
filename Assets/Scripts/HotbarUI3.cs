using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI3 : MonoBehaviour
{
    [System.Serializable]
    public class HotSlotUI
    {
        public Image icon;
        public TMP_Text label; // optional: slot number
    }

    [SerializeField] private HotSlotUI[] ui = new HotSlotUI[3];
    [SerializeField] private Transform followTarget; // player
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private PlayerHotbar3 hotbar;

    private void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + worldOffset;

        var inv = GameInventory9.I;
        if (inv == null || hotbar == null) return;

        for (int i = 0; i < 3; i++)
        {
            int itemIndex = hotbar.slots[i];
            if (ui[i].icon != null)
            {
                ui[i].icon.enabled = (itemIndex != -1);
                ui[i].icon.sprite = (itemIndex != -1) ? inv.items[itemIndex].icon : null;
            }
            if (ui[i].label != null) ui[i].label.text = (i + 1).ToString();
        }
    }

    public void SetVisible(bool v) => gameObject.SetActive(v);
}