using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    public GameObject ingredientPrefab;

    private GameObject currentlyDragging;
    private DraggableTool currentTool; // Cache the tool for performance
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ingredientPrefab != null)
        {
            Vector3 spawnPos = mainCamera.ScreenToWorldPoint(eventData.position);
            spawnPos.z = 0f;

            currentlyDragging = Instantiate(ingredientPrefab, spawnPos, Quaternion.identity);

            if (currentlyDragging.TryGetComponent(out currentTool))
            {
                // Tell the tool it is being dragged immediately
                currentTool.StartDraggingManually();
            }

            if (currentlyDragging.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.color = new Color(1f, 1f, 1f, 0.6f);
                sr.sortingOrder = 100; // Keep it high while dragging
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentlyDragging != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0f;
            currentlyDragging.transform.position = mousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
{
    if (currentlyDragging != null)
    {
        // --- FIX: Reset the color to solid white IMMEDIATELY ---
        if (currentlyDragging.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            sr.color = Color.white; // No more transparency
            sr.sortingOrder = 0;    // Drop it back to the board layer
        }

        if (currentTool != null)
        {
            currentTool.StopDraggingManually();
        }

        currentlyDragging = null;
        currentTool = null;
    }
}
}