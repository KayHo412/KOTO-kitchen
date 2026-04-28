using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LadleTouchHandler : MonoBehaviour
{
    public GameObject ladle; // Drag the Ladle UI object here
    public GameObject stirWindow; // Optional: assign StirWindow, or it will be found by name
    private RectTransform ladleRect;

    void Start()
    {
        if (ladle != null)
        {
            ladleRect = ladle.GetComponent<RectTransform>();
            ladle.SetActive(false); // Ensure it's hidden at start
        }

        if (stirWindow == null)
            stirWindow = GameObject.Find("StirWindow");
    }

    void Update()
    {
        if (ladle == null || ladleRect == null) return;

        if (stirWindow == null)
            stirWindow = GameObject.Find("StirWindow");

        if (stirWindow == null || !stirWindow.activeInHierarchy)
        {
            ladle.SetActive(false);
            return;
        }

        bool pressedDown = false;
        bool isPressed = false;
        bool released = false;
        Vector2 pointerPos = Vector2.zero;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            pressedDown = touch.press.wasPressedThisFrame;
            isPressed = touch.press.isPressed;
            released = touch.press.wasReleasedThisFrame;
            pointerPos = touch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            pressedDown = Mouse.current.leftButton.wasPressedThisFrame;
            isPressed = Mouse.current.leftButton.isPressed;
            released = Mouse.current.leftButton.wasReleasedThisFrame;
            pointerPos = Mouse.current.position.ReadValue();
        }

        if (!isPressed)
        {
            ladle.SetActive(false);
            return;
        }

        // Appear on touch/press, stay on the finger, disappear when not touching.
        if (pressedDown || isPressed)
            ladle.SetActive(true);

        if (isPressed && ladleRect != null)
            ladleRect.position = pointerPos;

        if (released)
            ladle.SetActive(false);
    }
}