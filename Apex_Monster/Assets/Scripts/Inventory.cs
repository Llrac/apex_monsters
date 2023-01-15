using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [HideInInspector] public List<GameObject> slots = new();
    [HideInInspector] public Collider2D slotCollider;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<SpriteRenderer>() != null)
            {
                slots.Add(child.gameObject);
            }
            else if (child.gameObject.GetComponent<Collider2D>() != null)
            {
                slotCollider = child.gameObject.GetComponent<Collider2D>();
            }
        }
    }
}
