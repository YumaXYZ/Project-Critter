using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro; 
using Newtonsoft.Json; 

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("El idioma por defecto si no se ha guardado ninguno. Ej: 'en', 'es'")]
    public string defaultLanguage = "en"; // Idioma predeterminado

    private Dictionary<string, string> localizedText;
    private string currentLanguageCode; // Almacena el código del idioma actual ("en", "es")

    // Evento para notificar cuando el idioma cambia
    public delegate void OnLanguageChanged();
    public static event OnLanguageChanged LanguageChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // IMPORTANTE: Hace que este objeto persista entre escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya existe otra instancia, destruye esta para asegurar un único Singleton
            Destroy(gameObject);
            return;
        }

        // Carga el idioma guardado o el idioma por defecto al iniciar
        LoadLanguage(PlayerPrefs.GetString("Language", defaultLanguage));
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        Debug.LogWarning($"LocalizationManager: Key '{key}' not found in current language '{currentLanguageCode}'. Returning key.");
        return key; 
    }

    public void LoadLanguage(string languageCode)
    {
        // Ruta al archivo JSON dentro de la carpeta Resources
        string filePath = Path.Combine("Localization", languageCode);
        TextAsset asset = Resources.Load<TextAsset>(filePath);

        if (asset != null)
        {
            try
            {
                localizedText = JsonConvert.DeserializeObject<Dictionary<string, string>>(asset.text);

                // Se asegura de que el diccionario no es null si el JSON está vacío o mal 
                if (localizedText == null)
                {
                    localizedText = new Dictionary<string, string>();
                    Debug.LogWarning($"LocalizationManager: Deserialized dictionary for '{languageCode}' was null. Initializing empty dictionary.");
                }

                currentLanguageCode = languageCode;
                PlayerPrefs.SetString("Language", languageCode);
                PlayerPrefs.Save(); // Guarda la preferencia del idioma
                Debug.Log($"LocalizationManager: Language loaded: {languageCode}");

                // Notifica a todos los suscriptores (ej. LocalizedText, OptionsManagerInGame)
                // que el idioma ha cambiado para que actualicen sus textos.
                LanguageChanged?.Invoke();
            }
            catch (JsonException e)
            {
                Debug.LogError($"LocalizationManager: Error parsing JSON for '{languageCode}' with Newtonsoft.Json: {e.Message}");
                localizedText = new Dictionary<string, string>(); // Inicializa vacío en caso de error de parseo
            }
        }
        else
        {
            Debug.LogError($"LocalizationManager: Failed to load language file for '{languageCode}' at path 'Resources/{filePath}'. Make sure the file exists and is in the correct format (JSON).");
            // Si el idioma solicitado no se encuentra, intenta cargar el predeterminado como fallback
            if (languageCode != defaultLanguage)
            {
                Debug.LogWarning($"LocalizationManager: Attempting to load default language '{defaultLanguage}'.");
                LoadLanguage(defaultLanguage);
            }
            else
            {
                // Si ni siquiera el idioma predeterminado se puede cargar, inicializa un diccionario vacío
                localizedText = new Dictionary<string, string>();
                Debug.LogError("LocalizationManager: No default or fallback language could be loaded. Localization will not work correctly.");
            }
        }
    }

    public string GetCurrentLanguage()
    {
        return currentLanguageCode;
    }
}