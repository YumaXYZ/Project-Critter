using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAchievementTrigger : MonoBehaviour
{
    // PONER EN UN GAMEOBJECT DONDE ESTE EL TRIGGER PARA UN LOGRO
    // p.e -> En un empty GameObject en la scene del Cementerio para el logro 1
    [Header("Scene Achievement Settings")]
    public string achievementId = "hello_zacarias"; // qué logro se triggerea, debe ser el mismo id que en la lista del AchievementManager
    public bool triggerOnStart = true; // se triggerea tan pronto se inicia la escena?
    public bool triggerOnce = true; // se triggerea una sola vez x partida?
    private bool hasTriggered = false;
    void Start()
    {
        if (triggerOnStart)
        {
            TriggerAchievement();
        }
    }

    // la función que se llama desde cualquier sitio para triggerear un logro
    public void TriggerAchievement()
    {
        if (AchievementManager.Instance != null && AchievementManager.Instance.IsAchievementUnlocked(achievementId))
        {
            Debug.Log($"Achievement '{achievementId}' already unlocked persistently. Skipping trigger.");
            return; // Ya está desbloqueado, no hacemos nada
        }

        // Si es un logro de un solo disparo POR SESIÓN Y YA SE DISPARÓ EN ESTA SESIÓN
        if (triggerOnce && hasTriggered) return;

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UnlockAchievement(achievementId);
            hasTriggered = true; // Marca que se disparó en esta sesión
        }
        else
        {
            Debug.LogError("Añade el AchievementManager al Main Menu!");
        }
    }

    // MÉTODO SI QUEREMOS UN LOGRO QUE SE TRIGGEREE AL ENTRAR EN UNA ZONA
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // la etiqueta que tenga el personaje
        {
            TriggerAchievement();
        }
    }
}
