using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class Monster : UsefulVariableClass
{
    [Tooltip("The type of monster. There are 10 unique types, each having strengths and weaknesses. " +
        "Chieftain, Warrior, Magic, Beast, Bird, Baby, Boss, Flat, Bobble, Mounted")]
    public string type = "";

    [Range(1, 3)] public int level = 1;
    [Range(1, 9)] public int attack = 1;
    [Range(1, 9)] public int startHealth = 1;
    [HideInInspector] public int currentHealth = 1;
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
    [HideInInspector] public int mergeProgress = 0;
    bool hasPlayedSFX = false;
    Vector2 mergePosition;
    GameObject mergeMonster;

    Collider2D slotCollider;
    [HideInInspector] public bool insideInventory = false;

    void Start()
    {
        startSize = transform.localScale;
        startHealth = type switch
        {
            "Chieftain" => 6,
            "Warrior" => 3,
            "Magic" => 3,
            "Beast" => 3,
            "Bird" => 3,
            "Baby" => 1,
            "Boss" => 3,
            "Flat" => 3,
            "Bobble" => 3,
            "Mounted" => 3,
            _ => startHealth,
        };
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
            if (child.gameObject.GetComponent<SpriteRenderer>() != null && child.name == "Glow")
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
            targetedLimb++;
        }
    }

    void Update()
    {
        mergeTimer += Time.deltaTime * Time.deltaTime * Application.targetFrameRate;
        if (mergeProgress == 1)
        {
            if (type != "Warrior")
                anim.SetTrigger("attack");
            if (mergeMonster.GetComponent<Monster>().type != "Warrior")
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
        //if (mergeProgress != 0)
        //    return;
        CheckMayDrag();
        if (!mayDrag)
            return;

        // Visual feedback
        if (type != "Warrior")
            anim.SetBool("isMoving", true);
        if (glow != null && gm.enableDragGlow)
            glow.SetActive(true);
        cursor.UpdateCursor(true);

        EnableAllTriggers(true);

        transform.localScale = new Vector3(startSize.x * dragSize, startSize.y * dragSize, 1);

        // Sorting Order
        gm.universalSortingOrderID++;
        int targetedLimb = 0;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = gm.universalSortingOrderID + initialSortingOrderID[targetedLimb];
            spriteRenderer.sortingLayerName = "Drag";
            targetedLimb++;
        }

        dragOffset = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
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
        if (type != "Warrior")
            anim.SetBool("isMoving", false);
        if (glow != null && gm.enableDragGlow)
            glow.SetActive(false);
        cursor.UpdateCursor(false);

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

        if (slotCollider != null)
        {
            InsertIntoNearbyInventorySlot();
        }
        else
        {
            transform.parent = null;
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
            mergeMonster = collision.transform.parent.gameObject;
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
        if (mousePos.x < -Width / 2 || mousePos.x > Width / 2 ||
            mousePos.y < -Height / 2 || mousePos.y > Height / 2)
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
        PolygonCollider2D mPolyCollider = null;
        foreach (Monster monster in FindObjectsOfType<Monster>())
        {
            if (monster.GetComponent<PolygonCollider2D>() != null)
            {
                mPolyCollider = monster.GetComponent<PolygonCollider2D>();
            }
            else
            {
                foreach (Transform child in monster.transform)
                {
                    if (child.gameObject.GetComponent<PolygonCollider2D>() != null)
                    {
                        mPolyCollider = child.gameObject.GetComponent<PolygonCollider2D>();
                    }
                }
            }
            mPolyCollider.isTrigger = enable;
        }

        FindObjectOfType<Inventory>().slotCollider.enabled = enable;
    }

    void InsertIntoNearbyInventorySlot()
    {
        float lastDistance = Mathf.Infinity;
        GameObject inventorySlot = null;
        foreach (GameObject newSlot in FindObjectOfType<Inventory>().slots)
        {
            if (Vector3.Distance(newSlot.transform.position, transform.position) < lastDistance)
            {
                lastDistance = Vector3.Distance(newSlot.transform.position, transform.position);
                inventorySlot = newSlot;
            }
        }
        transform.position = new Vector2(inventorySlot.transform.position.x + inventoryOffset.x, inventorySlot.transform.position.y + inventoryOffset.y);
        transform.localScale = startSize * inventorySize;
        transform.parent = inventorySlot.transform;
        insideInventory = true;
    }

    void BeginMerge() // enables merging
    {
        if (mergeMonster.GetComponent<Monster>().insideInventory)
        {
            FindObjectOfType<MonsterSpawner>().CheckMonstersAtScreenEdge();
            return;
        }

        mergePosition = transform.position;
        mergeProgress = 1;
        mergeTimer = 0;
    }

    public void LoseHealth(int amount = 1)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Delete();
        }
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}