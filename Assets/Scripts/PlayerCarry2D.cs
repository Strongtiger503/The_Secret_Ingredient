using UnityEngine;

public class PlayerCarry2D : MonoBehaviour
{
    [Header("Carried prefabs (slot 1/2/3)")]
    [SerializeField] private GameObject slot1Prefab;
    [SerializeField] private GameObject slot2Prefab;
    [SerializeField] private GameObject slot3Prefab;

    [SerializeField] private Transform holdPoint;
    [SerializeField] private float followLerp = 25f;

    private GameObject carriedInstance;
    private int carriedSlot = 0; // 0 = none, 1/2/3 = which item type

    public bool IsHolding => carriedInstance != null;
    public int CarriedSlot => carriedSlot;

    private void Awake()
    {
        if (holdPoint == null)
        {
            var hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0.6f, 0f, 0f);
            holdPoint = hp.transform;
        }
    }

    private void Update()
    {
        if (carriedInstance == null) return;

        Vector3 target = holdPoint.position;
        carriedInstance.transform.position = Vector3.Lerp(
            carriedInstance.transform.position,
            target,
            1f - Mathf.Exp(-followLerp * Time.deltaTime)
        );
    }

    public bool PickUpFromSlot(int slot)
    {
        if (IsHolding) return false;

        GameObject prefab = slot switch
        {
            1 => slot1Prefab,
            2 => slot2Prefab,
            3 => slot3Prefab,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogError($"PlayerCarry2D: Prefab for slot {slot} not assigned!");
            return false;
        }

        carriedSlot = slot;
        carriedInstance = Instantiate(prefab, holdPoint.position, Quaternion.identity);

        var rb = carriedInstance.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        var col = carriedInstance.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        return true;
    }

    // Deletes the carried item (deposit/consume)
    public void DepositAndDelete()
    {
        if (!IsHolding) return;

        Destroy(carriedInstance);
        carriedInstance = null;
        carriedSlot = 0;
    }
}