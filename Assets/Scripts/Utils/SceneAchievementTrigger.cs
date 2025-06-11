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
        // si era un logro que solo triggereaba una vez por partida y ya ha ocurrido
        if (triggerOnce && hasTriggered) return;

        // se asegura que el AchievementManager existe
        if (AchievementManager.Instance != null)
        {
            // le dice al manager que desbloquee el logro
            AchievementManager.Instance.UnlockAchievement(achievementId);
            hasTriggered = true;
        }
        else
        {
            // si el manager no está en el main menu
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
