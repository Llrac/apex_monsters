using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Collider2D slotCollider;

    [HideInInspector] public List<GameObject> slots = new();

    void Start()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            if (child.name == "Slot")
                slots.Add(child.gameObject);
        }
    }
}
