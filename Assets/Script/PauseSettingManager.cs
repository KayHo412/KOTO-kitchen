using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseSettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuWindow;

    [Header("Audio Settings")]
    public AudioMixer mainMixer; // Must be assigned in Inspector
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isPaused = false;
    private const string MusicVolKey = "MusicVolValue";
    private const string SfxVolKey = "SFXVolValue";
    private const float DefaultVol = 0.75f;

    void Start()
    {
        // Ensure menu is hidden and game is running at start
        if (pauseMenuWindow != null) pauseMenuWindow.SetActive(false);
        Time.timeScale = 1f;

        LoadAudioSettings();
    }

    private void LoadAudioSettings()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicVolKey, DefaultVol);
        float savedSfx = PlayerPrefs.GetFloat(SfxVolKey, DefaultVol);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSfx;

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSfx);
    }

    // Called by the 3-line button
    public void ToggleMenu()
    {
        isPaused = !isPaused;
        pauseMenuWindow.SetActive(isPaused);

        // Freeze/Unfreeze game logic (boiling timers, dragging, etc.)
        Time.timeScale = isPaused ? 0f : 1f;
    }

    // --- BUTTON ACTIONS ---

    public void RestartGame()
    {
        Time.timeScale = 1f; // ALWAYS reset time before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartingScreen"); // Ensure this matches your scene name
    }

    // --- SLIDER ACTIONS ---
    // Note: Mixers use Logarithmic scales (-80 to 20).
    // Set Sliders to Min 0.0001 and Max 1.

    public void SetMusicVolume(float value)
    {
        float safeValue = Mathf.Clamp(value, 0.0001f, 1f);
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(safeValue) * 20);

        PlayerPrefs.SetFloat(MusicVolKey, safeValue);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        float safeValue = Mathf.Clamp(value, 0.0001f, 1f);
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", Mathf.Log10(safeValue) * 20);

        PlayerPrefs.SetFloat(SfxVolKey, safeValue);
        PlayerPrefs.Save();
    }
}