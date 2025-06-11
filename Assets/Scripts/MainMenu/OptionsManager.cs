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

    // Reference to the MainMenuManager to allow returning to the main menu
    public MainMenuManager mainMenuManager; // Assign this in the Inspector!

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

        // Basic check to ensure the MainMenuManager reference is set.
        // This is crucial for navigating back to the main menu.
        if (mainMenuManager == null)
        {
            Debug.LogError("OptionsManager: MainMenuManager reference is not set! Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// This method is called by the MainMenuManager when the options panel is opened.
    /// It ensures the UI elements are set to their current values.
    /// </summary>
    public void SetupOptionsPanel()
    {
        LoadVolumeSettings(); // Load and apply volume settings when the panel becomes active
        // You would add any other setup logic for graphics, controls, etc., here.
    }

    /// <summary>
    /// Called when the volume slider's value changes.
    /// Updates the game's master volume and the displayed percentage.
    /// </summary>
    /// <param name="value">The new float value of the slider (0 to 1).</param>
    void OnVolumeSliderChanged(float value)
    {
        // AudioListener.volume controls the master volume for all sounds in Unity.
        AudioListener.volume = value;
        UpdateVolumeSliderAndText(value); // Update the displayed text
    }

    /// <summary>
    /// Called when the "Back" button in the options panel is clicked.
    /// Saves current settings and navigates back to the main menu.
    /// </summary>
    void OnOptionsBackButtonClicked()
    {
        if (mainMenuManager != null)
        {
            SaveVolumeSettings(); // Save changes before closing the panel
            // Tell the MainMenuManager to display the main menu panel.
            mainMenuManager.ShowPanel(mainMenuManager.menuPanel); 
        }
        else
        {
            Debug.LogError("OptionsManager: Cannot go back, MainMenuManager is not assigned!");
        }
    }

    /// <summary>
    /// Loads the saved volume setting from PlayerPrefs and applies it to the game.
    /// If no setting is found, it defaults to 1.0 (100%).
    /// </summary>
    void LoadVolumeSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f); // Default to 1.0 if no value saved
        
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume; // Set the slider's position
        }
        AudioListener.volume = savedVolume; // Apply the volume to the game
        UpdateVolumeSliderAndText(savedVolume); // Update the visual text representation
        Debug.Log($"Volume settings loaded: {savedVolume}");
    }

    /// <summary>
    /// Saves the current volume slider's value to PlayerPrefs.
    /// </summary>
    void SaveVolumeSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
            PlayerPrefs.Save(); // Ensures data is written to disk immediately
            Debug.Log($"Volume settings saved: {volumeSlider.value}");
        }
        else
        {
            Debug.LogWarning("VolumeSlider is null, cannot save volume settings.");
        }
    }

    /// <summary>
    /// Helper method to update the volume percentage text displayed next to the slider.
    /// </summary>
    /// <param name="volume">The current volume value (0 to 1).</param>
    private void UpdateVolumeSliderAndText(float volume)
    {
        if (volumeValueText != null)
        {
            // Convert the 0-1 float to a 0-100 percentage and format without decimals.
            volumeValueText.text = $"{(volume * 100):F0}%"; 
        }
    }
}
