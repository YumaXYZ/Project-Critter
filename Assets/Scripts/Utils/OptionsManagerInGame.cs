using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure TextMeshPro Essentials are imported

public class OptionsManagerInGame : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public TMP_Dropdown screenModeDropdown;

    [Header("Language Elements")]
    public TMP_Dropdown languageDropdown; 

    [Header("Localized Texts (in this script)")]
    public TMP_Text volumeLabelText;     
    public TMP_Text screenModeLabelText; 
    public TMP_Text languageLabelText;   
    public TMP_Text optionsTitleText;    
    
    private PauseManager pauseManagerInstance;

    void Awake()
    {
        pauseManagerInstance = FindObjectOfType<PauseManager>();
        if (pauseManagerInstance == null)
        {
            Debug.LogWarning("OptionsManagerInGame: No se encontró una instancia de PauseManager en la escena. Esto puede ser normal si no lo usas o si el panel de opciones no se activa a través de él.");
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
            // Options will be initialized in OnEnable/SetupOptionsPanel to ensure localized options
        }

        // NEW: Listener for the Language Dropdown
        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            // Options will be initialized in OnEnable/SetupOptionsPanel to ensure localized options
        }
    }

    void OnEnable()
    {
        // Set up the options panel every time it's enabled/opened
        SetupOptionsPanel();
        // Subscribe to the language changed event to update texts
        LocalizationManager.LanguageChanged += UpdateLocalizedTexts;
        UpdateLocalizedTexts(); // Update texts immediately when panel becomes active
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks when the object is disabled
        LocalizationManager.LanguageChanged -= UpdateLocalizedTexts;
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
        }
        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
        }
        // NEW: Remove listener for Language Dropdown
        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
    }

    /// <summary>
    /// Initializes the options for the Screen Mode Dropdown using localized texts.
    /// </summary>
    private void InitializeScreenModeDropdown()
    {
         // Temporarily remove listener to prevent recursive calls if setting value triggers OnScreenModeChanged
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);

            screenModeDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                LocalizationManager.Instance.GetLocalizedValue("fullscreen_option"),
                LocalizationManager.Instance.GetLocalizedValue("windowed_option")
            };
            screenModeDropdown.AddOptions(options);
            LoadScreenModeSettings(); // Load the saved value (this sets screenModeDropdown.value)

            // Re-add listener
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }

    /// <summary>
    /// NEW: Initializes the options for the Language Dropdown and sets the current language.
    /// </summary>
    private void InitializeLanguageDropdown()
    {
        if (languageDropdown != null && LocalizationManager.Instance != null)
        {
            // --- CRITICAL FIX: Temporarily remove the listener ---
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

            languageDropdown.ClearOptions();
            
            // Display names for languages (what the user sees)
            // Ensure these match the order of your language codes
            List<string> displayLanguages = new List<string>
            {
                LocalizationManager.Instance.GetLocalizedValue("en_lang"), // "English" localized
                LocalizationManager.Instance.GetLocalizedValue("es_lang")  // "Spanish" localized
            };

            // Internal language codes (must match your JSON file names, e.g., "en", "es")
            List<string> languageCodes = new List<string> { "en", "es" };

            languageDropdown.AddOptions(displayLanguages);

            // Set the dropdown's value to the current language
            string currentLangCode = LocalizationManager.Instance.GetCurrentLanguage();
            int currentIndex = languageCodes.IndexOf(currentLangCode);
            if (currentIndex != -1)
            {
                languageDropdown.value = currentIndex;
            }
            else
            {
                languageDropdown.value = 0; // Default to the first option if current is not found
            }

            // --- CRITICAL FIX: Re-add the listener ---
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }

    public void SetupOptionsPanel()
    {
        LoadVolumeSettings();
        LoadScreenModeSettings();
        // Ensure dropdowns are initialized/re-initialized with localized options
        InitializeScreenModeDropdown();
        InitializeLanguageDropdown();
    }

    void OnVolumeSliderChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeSliderAndText(value);
        SaveVolumeSettings();
    }

    void OnScreenModeChanged(int index)
    {
        bool isFullscreen = (index == 0); // Assuming 0 = Fullscreen, 1 = Windowed
        Screen.fullScreen = isFullscreen;
        Debug.Log($"Screen Mode set to: {(isFullscreen ? "Fullscreen" : "Windowed")}");
        SaveScreenModeSettings();
    }

    /// <summary>
    /// NEW: Called when the language selection in the dropdown changes.
    /// </summary>
    /// <param name="index">The index of the selected language in the dropdown.</param>
    void OnLanguageChanged(int index)
    {
        List<string> languageCodes = new List<string> { "en", "es" }; // Internal language codes
        if (index >= 0 && index < languageCodes.Count)
        {
            string selectedLanguageCode = languageCodes[index];
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.LoadLanguage(selectedLanguageCode);
                Debug.Log($"Language changed to: {selectedLanguageCode}");
                // Re-initialize dropdowns whose options' texts might have changed
                //InitializeScreenModeDropdown();
            }
        }
    }

    void LoadVolumeSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (volumeSlider != null) volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;
        UpdateVolumeSliderAndText(savedVolume);
        Debug.Log($"Volume settings loaded: {savedVolume}");
    }

    void LoadScreenModeSettings()
    {
        int savedScreenModeIndex = PlayerPrefs.GetInt("ScreenModeIndex", 0);
        if (screenModeDropdown != null) screenModeDropdown.value = savedScreenModeIndex;
        Screen.fullScreen = (savedScreenModeIndex == 0);
        Debug.Log($"Screen Mode selected: {(Screen.fullScreen ? "Fullscreen" : "Windowed")}");
    }

    void SaveVolumeSettings()
    {
        if (volumeSlider != null) PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.Save();
        Debug.Log("Volume settings saved!");
    }

    void SaveScreenModeSettings()
    {
        if (screenModeDropdown != null) PlayerPrefs.SetInt("ScreenModeIndex", screenModeDropdown.value);
        PlayerPrefs.Save();
        Debug.Log("Screen Mode settings saved!");
    }

    private void UpdateVolumeSliderAndText(float volume)
    {
        if (volumeValueText != null) volumeValueText.text = $"{(volume * 100):F0}";
    }

    private void UpdateLocalizedTexts()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning("LocalizationManager.Instance is null. Cannot update localized texts in OptionsManagerInGame.");
            return;
        }

        if (volumeLabelText != null)
        {
            volumeLabelText.text = LocalizationManager.Instance.GetLocalizedValue("volume_text");
        }
        if (screenModeLabelText != null)
        {
            screenModeLabelText.text = LocalizationManager.Instance.GetLocalizedValue("screen_mode_text");
        }
        if (languageLabelText != null)
        {
            languageLabelText.text = LocalizationManager.Instance.GetLocalizedValue("language_text");
        }
        if (optionsTitleText != null)
        {
            optionsTitleText.text = LocalizationManager.Instance.GetLocalizedValue("pause"); // Assuming "-PAUSE-" title
        }

        // Re-initialize dropdowns whose options' texts are localized
        InitializeScreenModeDropdown();
        InitializeLanguageDropdown();
    }
}