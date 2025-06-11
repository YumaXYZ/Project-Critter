using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Options Panel Elements")]
    public Slider volumeSlider;         // The UI Slider for volume control
    public TMP_Text volumeValueText;    // Text to display the current volume percentage (e.g., "100%")
    public Button optionsBackButton;    // Button to go back to the main menu

    [Header("Screen Mode Elements")]
    public TMP_Dropdown screenModeDropdown;

    [Header("Managers")]
    public MainMenuManager mainMenuManager; 

    void Awake()
    {
        // Add listeners for UI elements. These connect UI actions to C# methods.
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }
        
        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.AddListener(OnOptionsBackButtonClicked);
        }
        
         // Listener para el Dropdown de Modo de Pantalla
        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }

        // Basic check to ensure the MainMenuManager reference is set.
        // This is crucial for navigating back to the main menu.
        if (mainMenuManager == null)
        {
            Debug.LogError("OptionsManager: MainMenuManager reference is not set! Please assign it in the Inspector.");
        }
    }

    // método para limpiar el código justo antes de que el objeto OptionsManager se elimine de la escena. 
    // esto hace que el script deje de "escuchar" para un cambio en los campos si no se está en dicho panel. 
    // Evita fugas de memoria y errores.
    void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
        }
        if (optionsBackButton != null)
        {
            optionsBackButton.onClick.RemoveListener(OnOptionsBackButtonClicked);
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
        }
    }

    public void SetupOptionsPanel()
    {
        LoadVolumeSettings();
        LoadScreenModeSettings();
    }

    void OnVolumeSliderChanged(float value)
    {
        // AudioListener.volume controla el master volume en Unity
        AudioListener.volume = value;
        UpdateVolumeSliderAndText(value);
    }

    void OnScreenModeChanged(int index)
    {
        bool isFullscreen = (index == 0); // 0 = Pantalla Completa, 1 = Modo Ventana
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Screen Mode set to: {(isFullscreen ? "Fullscreen" : "Windowed")}");
    }

    void OnOptionsBackButtonClicked()
    {
        if (mainMenuManager != null)
        {
            SaveVolumeSettings();
            SaveScreenModeSettings();
            mainMenuManager.ShowPanel(mainMenuManager.menuPanel); 
        }
        else
        {
            Debug.LogError("MainMenuManager is not assigned!");
        }
    }

    void LoadVolumeSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f); // Default to 1.0 if no value saved
        if (volumeSlider != null) volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume; // Apply the volume to the game
        UpdateVolumeSliderAndText(savedVolume); // Update the visual text representation
        Debug.Log($"Volume settings loaded: {savedVolume}");
    }

    void LoadScreenModeSettings()
    {
        int savedScreenModeIndex = PlayerPrefs.GetInt("ScreenModeIndex", 0); 
        if (screenModeDropdown != null) screenModeDropdown.value = savedScreenModeIndex;
        Screen.fullScreen = (savedScreenModeIndex == 0); // Aplicar al inicio
        Debug.Log($"Screen Mode selected: {(Screen.fullScreen ? "Fullscreen" : "Windowed")}");
    }

    void SaveVolumeSettings()
    {
        if (volumeSlider != null) PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.Save();
    }
    
    void SaveScreenModeSettings()
    {
        if (screenModeDropdown != null) PlayerPrefs.SetInt("ScreenModeIndex", screenModeDropdown.value);
        PlayerPrefs.Save();
    }

    private void UpdateVolumeSliderAndText(float volume)
    {
        if (volumeValueText != null)  volumeValueText.text = $"{(volume * 100):F0}";
    }
}
