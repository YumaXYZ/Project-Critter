using UnityEngine;
using TMPro;

// Asegura que este script siempre estará en un GameObject que tenga un componente TMP_Text
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("La clave de localización para este texto. Debe coincidir con una clave en tus archivos JSON.")]
    public string localizationKey; // La clave que coincide con tu archivo JSON

    private TMP_Text textComponent; // Referencia al componente TextMeshPro

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        // Opcional: Asegurarse de que el LocalizationManager existe.
        // Si este script va a estar en la escena antes que el LocalizationManager se inicialice,
        // es mejor asegurarse de que el LocalizationManager existe en la escena.
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError($"LocalizedText: LocalizationManager.Instance es null para '{gameObject.name}'. Asegúrate de que el LocalizationManager está en la escena inicial y es un Singleton.");
        }
    }

    void OnEnable()
    {
        // Suscribirse al evento de cambio de idioma
        // Esto hará que el texto se actualice cada vez que el idioma cambie
        LocalizationManager.LanguageChanged += UpdateText;
        UpdateText(); // Actualizar el texto al habilitar el objeto
    }

    void OnDisable()
    {
        // Desuscribirse para evitar fugas de memoria o errores cuando el objeto es deshabilitado/destruido
        LocalizationManager.LanguageChanged -= UpdateText;
    }

    /// <summary>
    /// Método para actualizar el texto del componente TMPro usando la clave de localización.
    /// </summary>
    public void UpdateText()
    {
        if (textComponent != null && LocalizationManager.Instance != null && !string.IsNullOrEmpty(localizationKey))
        {
            // Obtiene el valor traducido y lo asigna al componente de texto
            textComponent.text = LocalizationManager.Instance.GetLocalizedValue(localizationKey);
        }
        else if (textComponent != null)
        {
            // Si la clave es nula o LocalizationManager no está listo, muestra la clave como fallback
            textComponent.text = localizationKey;
            Debug.LogWarning($"LocalizedText: No se pudo localizar el texto para la clave '{localizationKey}' en el objeto '{gameObject.name}'. LocalizationManager puede no estar listo o la clave es vacía.");
        }
    }
}
