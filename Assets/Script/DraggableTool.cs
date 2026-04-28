using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DraggableTool : MonoBehaviour
{
    [Header("General Snap Settings")]
    public bool shouldSnap = false;
    public string targetName = "CuttingBoard";
    public float snapDistance = 1.5f;

    [Header("Tool Tray / Snap Back")]
    public bool snapBackToHome = false;
    public Transform homePosition;
    public float returnSpeed = 10f;

    [Header("Pot Specific Settings")]
    public bool isPot = false;
    public Transform stoveSnapPoint;
    public Sprite waterPotSprite;
    public bool lockPotWhenPlacedOnStove = true;

    [Header("Cream Settings")]
    public bool isCream = false;

    [Header("Kettle Settings")]
    public float requiredPourTime = 2.0f;
    public ParticleSystem waterParticles;
    public Animator kettleAnim;

    [Header("Audio Settings")]
    public AudioSource localAudioSource;
    public AudioClip waterTickSound;

    [HideInInspector]
    public bool isCounted = false;

    private bool isDragging = false;
    private bool isOnStove = false;
    private bool hasWater = false;
    private float currentPourTimer = 0f;
    private bool isKettleTouchingPot = false;
    private bool isSnappedToTarget = false;
    private bool isForcedDragging = false;
    private bool isLockedOnStove = false;

    private Collider2D myCollider;
    private Camera mainCam;
    private SpriteRenderer sr;
    private int originalOrder;
    private GameObject cachedTarget;
    private Vector3 dragStartPosition;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        mainCam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        if (sr != null) originalOrder = sr.sortingOrder;
        if (waterParticles != null) waterParticles.Stop();

        cachedTarget = GameObject.Find(targetName);

        if (snapBackToHome && homePosition == null)
        {
            GameObject tempHome = new GameObject(gameObject.name + "_HomePos");
            tempHome.transform.position = transform.position;
            homePosition = tempHome.transform;
        }

        // Ensure kettle/water source always has a return point, even if snapBackToHome is off.
        if (this.CompareTag("WaterSource") && homePosition == null)
        {
            GameObject tempHome = new GameObject(gameObject.name + "_HomePos");
            tempHome.transform.position = transform.position;
            homePosition = tempHome.transform;
        }

        if (localAudioSource == null) localAudioSource = GetComponent<AudioSource>();
    }

    public void StartDraggingManually()
    {
        if (isPot && isLockedOnStove) return;

        dragStartPosition = transform.position;
        isDragging = true;
        isForcedDragging = true;

        if (isSnappedToTarget) ReleaseBoardOccupancy();

        isSnappedToTarget = false;
        if (cachedTarget == null) cachedTarget = GameObject.Find(targetName);
        if (sr != null) sr.sortingOrder = 100;
        Invoke("ReleaseForceDrag", 0.15f);
    }

    void ReleaseForceDrag() => isForcedDragging = false;

    public void StopDraggingManually()
    {
        isDragging = false;
        isForcedDragging = false;
        CancelInvoke("ReleaseForceDrag");

        if (sr != null)
        {
            sr.color = Color.white;
            sr.sortingOrder = originalOrder;
        }

        // Kettle behavior: return immediately when finger/mouse is released.
        if (this.CompareTag("WaterSource"))
        {
            ReturnWaterSourceHomeImmediately();
            return;
        }

        if (isCream)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            GameObject foundPot = null;
            foreach (var h in hits)
            {
                if (h.collider.name.Contains("Pot"))
                {
                    foundPot = h.collider.gameObject;
                    break;
                }
            }

            if (foundPot != null)
            {
                PotManager potScript = foundPot.GetComponent<PotManager>();
                if (potScript != null) potScript.NotifyCreamAdded(gameObject);
                Destroy(gameObject);
                return;
            }
            else if (!snapBackToHome)
            {
                Destroy(gameObject);
                return;
            }
        }

        // if (isPot) CheckStoveSnap();
        // else if (shouldSnap) CheckForGeneralSnap();

        if (isPot) CheckStoveSnap();
        else if (shouldSnap)
        {
            bool snapped = CheckForGeneralSnap();
        }
    }

    private void ReturnWaterSourceHomeImmediately()
    {
        if (homePosition != null)
            transform.position = homePosition.position;

        if (kettleAnim != null) kettleAnim.SetBool("isPouring", false);
        if (waterParticles != null)
        {
            waterParticles.Stop();
            waterParticles.Clear();
        }

        // Reset all pots' pour state immediately (same class can access private fields).
        DraggableTool[] tools = Object.FindObjectsByType<DraggableTool>(FindObjectsSortMode.None);
        foreach (DraggableTool tool in tools)
        {
            if (tool != null && tool.isPot)
            {
                tool.isKettleTouchingPot = false;
                tool.currentPourTimer = 0f;
            }
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);

            if (!isForcedDragging && !Mouse.current.leftButton.isPressed)
            {
                StopDraggingManually();
            }
        }
        else
        {
            HandleMovementCheck();

            if (snapBackToHome && !isSnappedToTarget && homePosition != null)
            {
                transform.position = Vector3.Lerp(transform.position, homePosition.position, Time.deltaTime * returnSpeed);
            }
        }

        if (isPot && isOnStove && !hasWater && isKettleTouchingPot)
        {
            currentPourTimer += Time.deltaTime;
            if (currentPourTimer >= requiredPourTime) FillPot();
        }
    }

    void HandleMovementCheck()
    {
        if (isPot && isLockedOnStove) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameObject stirWindow = GameObject.Find("StirWindow");
            if (stirWindow != null && stirWindow.activeInHierarchy) return;

            Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                StartDraggingManually();
            }
        }
    }

    public bool CheckForGeneralSnap()
    {
        if (cachedTarget == null) cachedTarget = GameObject.Find(targetName);
        PotatoSlicer slicer = GetComponent<PotatoSlicer>();

        if (cachedTarget != null)
        {
            CuttingBoardStatus board = cachedTarget.GetComponent<CuttingBoardStatus>();
            float distance = Vector2.Distance(transform.position, cachedTarget.transform.position);

            if (distance < snapDistance && board == null)
            {
                Debug.LogWarning("CuttingBoardStatus is missing on target board. Snap blocked.");
                isSnappedToTarget = false;
                if (slicer != null) slicer.isOnBoard = false;
                return false;
            }

            if (distance < snapDistance && board != null && board.isOccupied)
            {
                isSnappedToTarget = false;
                if (slicer != null) slicer.isOnBoard = false;

                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification(Localization.Get("BoardOccupied"));

                Destroy(gameObject);

                return false;
            }

            if (distance < snapDistance && board != null && !board.isOccupied)
            {
                transform.position = new Vector3(cachedTarget.transform.position.x, cachedTarget.transform.position.y, 0);
                isSnappedToTarget = true;
                if (board != null) board.SetOccupied(true);
                if (slicer != null) slicer.isOnBoard = true;

                if (this.CompareTag("Ingredient"))
                {
                    TutorialController tutorial = TutorialController.Instance;
                    if (tutorial != null && tutorial.GetCurrentTarget() == cachedTarget)
                    {
                        tutorial.ProceedNextStep(this.gameObject);
                    }
                }

                return true;
            }
        }
        isSnappedToTarget = false;
        if (slicer != null) slicer.isOnBoard = false;
        return false;
    }

    private void ReleaseBoardOccupancy()
    {
        if (cachedTarget == null) cachedTarget = GameObject.Find(targetName);
        if (cachedTarget != null)
        {
            CuttingBoardStatus board = cachedTarget.GetComponent<CuttingBoardStatus>();
            if (board != null) board.SetOccupied(false);
        }
    }

    void FillPot()
    {
        if (waterPotSprite != null)
        {
            sr.sprite = waterPotSprite;
            hasWater = true;
            PotManager pot = GetComponent<PotManager>();
            if (pot != null) pot.SetWaterStatus(true);
            if (waterParticles != null) { waterParticles.Stop(); waterParticles.Clear(); }
            if (localAudioSource != null && waterTickSound != null) localAudioSource.PlayOneShot(waterTickSound);

            // AUTO-ADVANCE TUTORIAL
            TutorialController tutorial = Object.FindFirstObjectByType<TutorialController>();
            if (tutorial != null) tutorial.ProceedNextStep();
        }
    }

    void CheckStoveSnap()
    {
        if (stoveSnapPoint == null) return;
        float distance = Vector2.Distance(transform.position, stoveSnapPoint.position);
        var steamSys = Object.FindFirstObjectByType<SteamSystem>();
        PotManager pot = GetComponent<PotManager>();

        if (distance < snapDistance)
        {
            transform.position = new Vector3(stoveSnapPoint.position.x, stoveSnapPoint.position.y, 0);
            isOnStove = true;
            if (lockPotWhenPlacedOnStove) isLockedOnStove = true;

            if (!isCounted)
            {
                TutorialController tutorial = TutorialController.Instance;
                if (tutorial != null)
                {
                    int beforeStep = tutorial.GetCurrentStepIndex();
                    tutorial.ProceedNextStep(this.gameObject);
                    int afterStep = tutorial.GetCurrentStepIndex();
                    if (afterStep != beforeStep)
                    {
                        isCounted = true;
                    }
                }
                else
                {
                    isCounted = true;
                }
            }

            if (steamSys != null) steamSys.UpdatePotStatus(true);
            if (BackgroundManager.Instance != null) BackgroundManager.Instance.SwitchToStoveOn();
        }
        else
        {
            isOnStove = false;
            if (steamSys != null) steamSys.UpdatePotStatus(false);
            if (pot != null) pot.SetWaterReady(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPotCollider(other) && this.CompareTag("Ingredient") && !isCream)
        {
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowNotification(Localization.Get("SliceFoodFirst"));

            Destroy(gameObject);
        }

        if (this.CompareTag("WaterSource") && IsPotCollider(other))
        {
            if (kettleAnim != null) kettleAnim.SetBool("isPouring", true);
            if (waterParticles != null) waterParticles.Play();

            DraggableTool potComp = other.GetComponent<DraggableTool>();
            if(potComp != null && potComp.isPot) potComp.isKettleTouchingPot = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (this.CompareTag("WaterSource") && IsPotCollider(other))
        {
            if (kettleAnim != null) kettleAnim.SetBool("isPouring", false);
            if (waterParticles != null) { waterParticles.Stop(); waterParticles.Clear(); }

            DraggableTool potComp = other.GetComponent<DraggableTool>();
            if(potComp != null && potComp.isPot)
            {
                potComp.isKettleTouchingPot = false;
                potComp.currentPourTimer = 0f;
            }
        }
    }

    private bool IsPotCollider(Collider2D other)
    {
        if (other == null) return false;

        if (other.TryGetComponent<PotManager>(out _)) return true;

        DraggableTool otherTool = other.GetComponent<DraggableTool>();
        return otherTool != null && otherTool.isPot;
    }
}