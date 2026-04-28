using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Use this for the score display
using System;
using UnityEngine.SceneManagement;

public class MinigamePlayer : MonoBehaviour
{
    public static event Action OnPlayerDied;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathScoreText;

    [Header("Audio")]
    public AudioSource backgroundMusicSource;
    public AudioSource deathAudioSource;
    public AudioClip deathPanelOpenClip;

    [Header("Movement")]
    public float dragSensitivity = 1.8f;

    private float survivalTime;
    private bool isDead = false;

    public bool IsDead => isDead;
    public int CurrentScore => Mathf.FloorToInt(survivalTime);

    void Start()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isDead) return;

        survivalTime += Time.deltaTime;
        if (scoreText != null) scoreText.text = Localization.Get("Score") + ": " + CurrentScore.ToString();

        // New Input System Touch Follow
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed)
        {
            Vector2 currentScreenPos = pointer.position.ReadValue();
            Vector2 screenDelta = pointer.delta.ReadValue();

            if (screenDelta != Vector2.zero && Camera.main != null)
            {
                Vector3 currentWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(currentScreenPos.x, currentScreenPos.y, 10f));
                Vector3 previousWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(currentScreenPos.x - screenDelta.x, currentScreenPos.y - screenDelta.y, 10f));
                Vector3 worldDelta = currentWorldPos - previousWorldPos;

                transform.position += new Vector3(worldDelta.x, worldDelta.y, 0f) * dragSensitivity;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (isDead) return;

            isDead = true;
            OnPlayerDied?.Invoke();

            if (deathPanel != null) deathPanel.SetActive(true);
            if (deathScoreText != null) deathScoreText.text = Localization.Get("Score") + ": " + CurrentScore.ToString();

            StopBGMAndPlayDeathSFX();

            Time.timeScale = 0f;
            Debug.Log("Game Over! Score: " + CurrentScore);
        }
    }

    private void StopBGMAndPlayDeathSFX()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }

        if (deathAudioSource != null && deathPanelOpenClip != null)
        {
            deathAudioSource.PlayOneShot(deathPanelOpenClip);
        }
    }

    public void RestartMinigame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartingScreen");
    }
}