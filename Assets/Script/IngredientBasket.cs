using UnityEngine;
using UnityEngine.InputSystem;

public class IngredientBasket : MonoBehaviour
{
    public GameObject potatoPrefab;
    private Camera mainCam;

    void Start() => mainCam = Camera.main;

    void Update()
    {
        // Changed to 'isPressed' check to ensure we catch the hold
        // even if the click started slightly outside the collider
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                SpawnAndGrab(mousePos);
            }
        }
    }

    void SpawnAndGrab(Vector2 pos)
    {
        // 1. Spawn the potato
        GameObject newPotato = Instantiate(potatoPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

        // 2. Get the tool component
        DraggableTool tool = newPotato.GetComponent<DraggableTool>();

        if (tool != null)
        {
            // 3. Force the position immediately so it doesn't jump
            newPotato.transform.position = new Vector3(pos.x, pos.y, 0);

            // 4. Start the drag
            tool.StartDraggingManually();
        }
    }
}