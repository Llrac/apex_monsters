using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCollider : MonoBehaviour
{
    GameObject inventory;
    Vector2 dragOffset;
    Vector2 mousePos;

    private void Start()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        inventory = FindObjectOfType<Inventory>().gameObject;
    }

    public void OnDown()
    {

    }
    public void OnDrag()
    {
        transform.position = new Vector2(transform.position.x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    }
}
