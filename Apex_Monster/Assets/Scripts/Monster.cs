using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Monster : MonoBehaviour
{
    [Tooltip("The type of monster. There are 10 unique types. " +
        "Chieftain, Warrior, Magic, Beast, Bird, Baby, Boss, Flat, Bobble, Mounted")]
    public string type = "";

    [Tooltip("The battle style of a monster. There are 4 unique battle styles: " +
        "Flat & Warrior & Chieftain = Fighter, Magic & Mounted = Flying, Bobble & Beast & Boss = Huge, Baby & Bird = Neutral")]
    public string battleStyle = "";

    public int monsterID = 0;

    [Header("Stats")]
    [Range(1, 7)] public int attack = 1;
    [Range(1, 17)] public int startHealth = 1;
    [HideInInspector] public int currentHealth = 1;
    [HideInInspector] public int level = 1;

    [Header("Size")]
    [Range(1f, 1.25f)] public float dragSize = 1.25f;
    public Vector2 inventoryOffset;
    public float inventorySize = 1f;

    [HideInInspector] public Vector3 startSize;
    Vector3 mousePos;
    Vector3 dragOffset;
    readonly List<int> initialSortingOrderID = new();

    Animator anim;
    GameObject glow;
    GameManager gm;
    CursorScript cursor;
    SpriteRenderer[] spriteRenderers;

    bool mayDrag = true;
    float mergeTimer = 10;
    bool hasPlayedSFX = false;
    Vector2 mergePosition;
    GameObject mergeMonster;
    [HideInInspector] public int mergeProgress = 0;

    Vector2 swapPosition;
    Collider2D slotCollider;
    [HideInInspector] public bool insideInventory = false;

    void Start()
    {
        startSize = transform.localScale;
        currentHealth = startHealth;
        GetComponents();
    }

    void GetComponents()
    {
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        cursor = FindObjectOfType<CursorScript>();

        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        rb2d.gravityScale = 0;

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<SpriteRenderer>() && child.name == "Glow")
            {
                glow = child.gameObject;
                glow.SetActive(false);
            }
        }

        // Goes through each limb and saves their sorting order ID
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        int targetedLimb = 0;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            initialSortingOrderID.Add(targetedLimb);
            initialSortingOrderID[targetedLimb] = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = gm.universalSortingOrderID + initialSortingOrderID[targetedLimb];
            targetedLimb++;
        }
    }

    void Update()
    {
        mergeTimer += Time.deltaTime * Time.deltaTime * Application.targetFrameRate;
        if (mergeProgress == 1)
        {
            anim.SetTrigger("attack");
            mergeMonster.GetComponent<Animator>().SetTrigger("attack");
            mergeMonster.transform.localScale = new Vector2(mergeMonster.transform.localScale.x * -1, mergeMonster.transform.localScale.y);
            hasPlayedSFX = false;

            mergeProgress = 2;
        }
        if (mergeProgress == 2 && mergeTimer < gm.positionDuringMergeCurve[gm.positionDuringMergeCurve.length - 1].time)
        {
            transform.position = new Vector2(mergePosition.x + gm.positionDuringMergeCurve.Evaluate(mergeTimer), mergePosition.y);
            mergeMonster.transform.position = new Vector2(mergePosition.x - gm.positionDuringMergeCurve.Evaluate(mergeTimer), mergePosition.y);
        }
        if (mergeProgress == 2 && !hasPlayedSFX && mergeTimer > gm.positionDuringMergeCurve[gm.positionDuringMergeCurve.length - 1].time * FindObjectOfType<AudioManager>().sfxDelay)
        {
            FindObjectOfType<AudioManager>().GetNextPopSFX();
            hasPlayedSFX = true;
        }
        if (mergeProgress == 2 && mergeTimer >= gm.positionDuringMergeCurve[gm.positionDuringMergeCurve.length - 1].time)
        {
            mergeMonster.transform.localScale = new Vector2(Mathf.Abs(mergeMonster.transform.localScale.x), mergeMonster.transform.localScale.y);
            FindObjectOfType<MonsterSpawner>().Merge(GetComponent<Monster>(), mergeMonster.GetComponent<Monster>());
            mergeMonster = null;

            mergeProgress = 0;
        }
    }

    void OnMouseDown()
    {
        CheckMayDrag();
        if (!mayDrag)
            return;

        // Visual feedback
        anim.SetBool("isMoving", true);
        if (glow != null && gm.enableDragGlow)
            glow.SetActive(true);
        cursor?.UpdateCursor(true);

        EnableAllTriggers(true);

        transform.localScale = new Vector3(startSize.x * dragSize, startSize.y * dragSize, 1);
        dragOffset = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
        swapPosition = new Vector3(mousePos.x, mousePos.y, 0) - dragOffset;

        // Sorting Order
        gm.universalSortingOrderID++;
        int targetedLimb = 0;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = gm.universalSortingOrderID + initialSortingOrderID[targetedLimb];
            spriteRenderer.sortingLayerName = "Drag";
            targetedLimb++;
        }
    }

    void OnMouseDrag()
    {
        //if (mergeProgress != 0)
        //    return;

        CheckMayDrag();
        if (!mayDrag)
            return;

        // Drag mechanics
        transform.position = new Vector3(mousePos.x, mousePos.y, 0) - dragOffset;
    }

    void OnMouseUp()
    {
        //if (mergeProgress != 0)
        //    return;

        // Visual feedback
        anim.SetBool("isMoving", false);
        if (glow != null && gm.enableDragGlow)
            glow.SetActive(false);
        cursor?.UpdateCursor(false);

        int targetedLimb = 0;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = gm.universalSortingOrderID + initialSortingOrderID[targetedLimb];
            spriteRenderer.sortingLayerName = "Default";
            targetedLimb++;
        }

        // Return scale
        if (gameObject.transform.localScale.x < 0)
        {
            gameObject.transform.localScale = new Vector3(-startSize.x, startSize.y, startSize.z);
        }
        else if (gameObject.transform.localScale.x > 0)
        {
            gameObject.transform.localScale = new Vector3(startSize.x, startSize.y, startSize.z);
        }

        if (FindObjectOfType<Inventory>() == null) { Debug.Log("missing inventory"); return; }

        if (slotCollider != null)
        {
            InsertIntoNearbyInventorySlot();
        }
        else
        {
            insideInventory = false;

            if (mergeMonster != null)
            {
                BeginMerge();
            }
        }

        EnableAllTriggers(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == FindObjectOfType<Inventory>().slotCollider)
        {
            slotCollider = collision;
        }
        else
        {
            mergeMonster = collision.GetComponentInParent<Monster>().gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == FindObjectOfType<Inventory>().slotCollider)
        {
            slotCollider = null;
        }
        else if (mergeProgress == 0)
        {
            mergeMonster = null;
        }
    }

    void CheckMayDrag()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x < -Camera.main.orthographicSize * Camera.main.aspect || mousePos.x > Camera.main.orthographicSize * Camera.main.aspect * 2 ||
            mousePos.y < -Camera.main.orthographicSize || mousePos.y > Camera.main.orthographicSize * 2)
        {
            mayDrag = false;
        }
        else
        {
            mayDrag = true;
        }
    }

    void EnableAllTriggers(bool enable)
    {
        foreach (Monster monster in FindObjectsOfType<Monster>())
        {
            PolygonCollider2D[] colliders = GetComponentsInChildren<PolygonCollider2D>();
            foreach (PolygonCollider2D collider in colliders)
            {
                collider.isTrigger = enable;
            }
        }
        if (FindObjectOfType<Inventory>() != null)
            FindObjectOfType<Inventory>().slotCollider.enabled = enable;
    }

    void InsertIntoNearbyInventorySlot()
    {
        float lastDistance = Mathf.Infinity;
        GameObject inventorySlot = null;
        foreach (GameObject slot in FindObjectOfType<Inventory>().slots)
        {
            if (Vector3.Distance(slot.transform.position, transform.position) < lastDistance)
            {
                lastDistance = Vector3.Distance(slot.transform.position, transform.position);
                inventorySlot = slot;
            }
        }

        if (inventorySlot.GetComponentInChildren<Monster>()) // swap monster with the one inside occupied slot
        {
            Monster swapMonster = inventorySlot.GetComponentInChildren<Monster>();
            swapMonster.transform.position = swapPosition;
            swapMonster.transform.localScale = swapMonster.startSize;
        }
        transform.position = new Vector2(inventorySlot.transform.position.x + inventoryOffset.x, inventorySlot.transform.position.y + inventoryOffset.y);
        transform.localScale = startSize * inventorySize;
        insideInventory = true;
    }

    void BeginMerge() // enables merging
    {
        if (mergeMonster.GetComponent<Monster>().insideInventory)
        {
            FindObjectOfType<MonsterSpawner>().UpdateMonsterPositions();
            return;
        }

        mergePosition = transform.position;
        mergeProgress = 1;
        mergeTimer = 0;
    }

    //public void LoseHealth(int amount = 1)
    //{
    //    currentHealth -= amount;
    //    if (currentHealth <= 0)
    //    {
    //        Delete();
    //    }
    //}

    public void Delete()
    {
        Destroy(gameObject);
    }
}