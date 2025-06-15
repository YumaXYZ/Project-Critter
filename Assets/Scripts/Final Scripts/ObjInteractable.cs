using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjInteractable : MonoBehaviour
{
    public string interactionLocalizationKey = "default_interaction_message";

    public string GetMessage()
    {
        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(interactionLocalizationKey))
        {
            // Usamos el LocalizationManager para obtener el texto traducido.
            return LocalizationManager.Instance.GetLocalizedValue(interactionLocalizationKey);
        }
        else
        {
            // Si el LocalizationManager no está disponible o la clave está vacía,
            // devolvemos la clave misma o un mensaje de error.
            Debug.LogWarning($"ObjInteractable: LocalizationManager no encontrado o 'interactionLocalizationKey' vacío para el objeto {gameObject.name}. Devolviendo la clave: {interactionLocalizationKey}");
            return interactionLocalizationKey; // Puedes cambiar esto a un mensaje de error genérico si lo prefieres
        }
    }
}
