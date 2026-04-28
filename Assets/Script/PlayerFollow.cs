using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Vector3 targetPosition;

    void Update()
    {
        // On a big screen, we move toward where the finger is held down
        if (Input.GetMouseButton(0))
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition.z = 0; // Keep it in 2D

            // Smoothly glide toward the finger
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Hit!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}