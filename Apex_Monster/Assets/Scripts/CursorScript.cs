using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [SerializeField] Sprite dragSprite = null;
    Sprite defaultSprite;

    [SerializeField] Vector2 offset = new(0.1f, -.2f);
    Vector3 mousePos;

    GameManager gm;
    SpriteRenderer sr;

    Rect screenRect;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        sr = GetComponent<SpriteRenderer>();
        defaultSprite = sr.sprite;
        screenRect = new(0, 0, Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);

        if (screenRect.Contains(Input.mousePosition))
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    public void UpdateCursor(bool onMouseDown)
    {
        if (onMouseDown && dragSprite != null)
        {
            sr.sprite = dragSprite;
        }
        else
        {
            sr.sprite = defaultSprite;
        }
        sr.sortingOrder = gm.universalSortingOrderID + 10;
    }
}
