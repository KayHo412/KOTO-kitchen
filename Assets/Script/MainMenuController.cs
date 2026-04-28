using UnityEngine;
using UnityEngine.SceneManagement; // This line is required to change scenes
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour
{
    public GameObject AboutPanel;
    public GameObject ExitButton;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider musicSlider;

    private const string MusicVolKey = "MusicVolValue";
    private const float DefaultVol = 0.75f;

    private void Start()
    {
        if (AboutPanel != null) AboutPanel.SetActive(false);
        if (ExitButton != null) ExitButton.SetActive(false);

        LoadMusicVolume();
    }

    private void LoadMusicVolume()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicVolKey, DefaultVol);

        if (musicSlider != null) musicSlider.value = savedMusic;
        SetMusicVolume(savedMusic);
    }

    public void StartGame()
    {
        // Loads the scene by its name.
        // Ensure the spelling matches "SampleScene" exactly!
        SceneManager.LoadScene("SampleScene");
    }

    public void StartMinigame()
    {
        SceneManager.LoadScene("MinigameScene");
    }

    // This function will be called by your Quit Button
    public void QuitGame()
    {
        Debug.Log("User has quit the game.");
        Application.Quit();
    }

    public void OpenAboutPanel()
    {
        Debug.Log("About panel opened.");
        if (AboutPanel != null) AboutPanel.SetActive(true);
        if (ExitButton != null) ExitButton.SetActive(true);
    }

    public void CloseAboutPanel()
    {
        Debug.Log("About panel closed.");
        if (AboutPanel != null) AboutPanel.SetActive(false);
        if (ExitButton != null) ExitButton.SetActive(false);
    }

    public void SetMusicVolume(float value)
    {
        float safeValue = Mathf.Clamp(value, 0.0001f, 1f);
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(safeValue) * 20);

        PlayerPrefs.SetFloat(MusicVolKey, safeValue);
        PlayerPrefs.Save();
    }


}