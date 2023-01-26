using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]

public class Monster : MonoBehaviour
{
    [Tooltip("The type of monster. There are 10 unique types. " +
        "Chieftain, Warrior, Magic, Beast, Bird, Baby, Boss, Flat, Bobble, Mounted")]
    public string type = "";

    [Tooltip("The battle style of a monster. There are 4 unique battle styles: " +
        "Flat & Warrior & Chieftain = Fighter, Magic & Mounted = Airborne, Bobble & Beast & Boss = Huge")]
    public string battleStyle = "";
    
    public int monsterID = 0;

    [Header("Stats")]
    [Range(1, 9)] public int attack = 1;
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
    GameObject inventorySlot = null;
    float startZPosition;
    [HideInInspector] public bool insideInventory = false;

    // Monster Canvas Variables
    GameObject monsterCanvas;
    GameObject attackCanvas;
    GameObject healthCanvas;
    GameObject levelCanvas;

    void Start()
    {
        SetupMonsterCanvas();
        startSize = transform.localScale;
        currentHealth = startHealth;
        //startZPosition = 0;
        GetComponents();
    }

    void SetupMonsterCanvas()
    {
        if (monsterCanvas != null) { return; }

        monsterCanvas = Instantiate(FindObjectOfType<MonsterSpawner>().monsterCanvas, transform);
        monsterCanvas.GetComponent<RectTransform>().position += type switch
        {
            "Chieftain" => new Vector3(0.1f, -0.3f),
            "Warrior" => new Vector3(0.1f, -0.35f),
            "Magic" => new Vector3(0, -0.35f),
            "Beast" => new Vector3(0, -0.65f),
            "Bird" => new Vector3(0.1f, -0.5f),
            "Baby" => Vector3.zero,
            "Boss" => new Vector3(0.05f, -0.55f),
            "Flat" => new Vector3(0, -0.45f),
            "Bobble" => new Vector3(0.1f, -0.25f),
            "Mounted" => new Vector3(-0.25f, -0.7f),
            _ => Vector3.zero
        };
        UpdateMonsterCanvas();
    }

    public void UpdateMonsterCanvas()
    {
        if (!monsterCanvas)
        {
            SetupMonsterCanvas();
            return;
        }

        foreach (RectTransform child in monsterCanvas.GetComponentsInChildren<RectTransform>())
        {
            if (child.name == "Horizontal Layout Group" && type == "Baby")
            {
                child.gameObject.SetActive(false);
            }
            if (child.name == "Attack")
            {
                attackCanvas = child.gameObject;
                attackCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = attack.ToString();
            }
            else if (child.name == "Health")
            {
                healthCanvas = child.gameObject;
                healthCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = startHealth.ToString();
            }
            else if (child.name == "Level")
            {
                levelCanvas = child.gameObject;
                levelCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = level.ToString();
            }
        }
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
            FindObjectOfType<AudioManager>().PlayPopSFX();
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
        //swapPosition = new Vector3(mousePos.x, mousePos.y, 0) - dragOffset;

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
        CheckMayDrag();
        if (!mayDrag)
            return;

        transform.position = new Vector3(mousePos.x, mousePos.y, 0) - dragOffset;
    }

    void OnMouseUp()
    {
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

        if (FindObjectOfType<Inventory>() == null) { Debug.LogWarning("missing inventory"); return; }

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
        if (!FindObjectOfType<Inventory>()) { return; }

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
        if (!FindObjectOfType<Inventory>()) { return; }

        if (collision == FindObjectOfType<Inventory>().slotCollider )
        {
            slotCollider = null;
        }
        else
        {
            if (mergeProgress == 0)
            mergeMonster = null;
        }
    }

    void CheckMayDrag()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x < -Camera.main.orthographicSize * Camera.main.aspect || mousePos.x > Camera.main.orthographicSize * Camera.main.aspect ||
            mousePos.y < -Camera.main.orthographicSize || mousePos.y > Camera.main.orthographicSize ||
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BattleMonsters")
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
        foreach (GameObject slot in FindObjectOfType<Inventory>().slots)
        {
            if (Vector3.Distance(slot.transform.position, transform.position) < lastDistance)
            {
                lastDistance = Vector3.Distance(slot.transform.position, transform.position);
                inventorySlot = slot;
            }
        }
        //if (inventorySlot.transform.position.z != startZPosition) // swap monster with the one inside occupied slot
        //{
        //    Debug.Log(inventorySlot.transform.position.z);
        //    Debug.Log(startZPosition);
        //    lastDistance = Mathf.Infinity;
        //    Monster swapMonster = null;
        //    foreach (Monster monster in FindObjectsOfType<Monster>())
        //    {
        //        if (Vector3.Distance(monster.transform.position, transform.position) < lastDistance && monster != this && monster.insideInventory)
        //        {
        //            lastDistance = Vector3.Distance(monster.transform.position, transform.position);
        //            swapMonster = monster;
        //        }
        //    }
        //    swapMonster.transform.position = swapPosition;
        //    swapMonster.transform.localScale = swapMonster.startSize;
        //    swapMonster.insideInventory = false;
        //}
        //inventorySlot.transform.position = new Vector3(inventorySlot.transform.position.x, inventorySlot.transform.position.y, -1);
        transform.position = new Vector2(inventorySlot.transform.position.x + inventoryOffset.x, inventorySlot.transform.position.y + inventoryOffset.y);
        transform.localScale = startSize * inventorySize;
        
        insideInventory = true;
    }

    void BeginMerge() // enables merging
    {
        if (mergeMonster.GetComponent<Monster>().insideInventory)
        {
            FindObjectOfType<GameManager>().UpdateScreen();
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