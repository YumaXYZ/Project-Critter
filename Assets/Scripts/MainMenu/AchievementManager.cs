using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementManager : MonoBehaviour
{
    // SE PONE EN UN GAMEOBJECT EN EL MAIN MENU 
    // singleton pattern (asegura que una clase sólo tenga una instancia y tenga un único punto de acceso global)
    public static AchievementManager Instance;

    [Header("Achievement Settings")]
    public GameObject achievementPopupPrefab; //el popup prefab aquí en el inspector
    public Transform achievementContainer; // donde aparecen los logros (canvas)

    [Header("Achievement List")]
    public List<Achievement> achievements = new(); // la lista aparece en el inspector

    // otros scripts pueden escucharlo para cuando un logro se desbloquea
    public static event Action<Achievement> OnAchievementUnlocked;

    private void Awake()
    {
        // singleton - sólo existe un AchievementManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // no se destruye al cambiar escenas
            InitializeAchievements(); // setup de logros
            LoadAchievements(); // se cargan los logros desbloqueados anteriormente
        }
        else
        {
            Destroy(gameObject); // destruye duplicados
        }
    }

    private void InitializeAchievements()
    {
        // se limpia la lista si estamos reiniciando
        achievements.Clear();

        achievements.Add(new Achievement(
            "hello_zacarias",
            "Hello, Zacarías!",
            "Meet Zacarías for the first time"
        ));
    }

    // se puede llamar desde cualquier sitio con: AchievementManager.Instance.UnlockAchievement("achievementId")
    public void UnlockAchievement(string achievementId)
    {
        // se busca el logro con ese id en la lista
        Achievement achievement = achievements.Find(a => a.id == achievementId);

        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true; // se marca como desbloqueado
            achievement.unlockedDate = DateTime.Now; // fecha de ahora

            Debug.Log($"Achievement unlocked -> {achievement.title}");

            // comunica a otros scripts que ha sido desbloqueado el logro
            OnAchievementUnlocked?.Invoke(achievement);

            // ShowAchievementPopup(achievement); // se muestra el popup

            SaveAchievements(); // se guarda el progreso cuando se cierra el juego
        }
    }

    // crea y muestra el popup UI
    // private void ShowAchievementPopup(Achievement achievement)
    // {
    //     // sólo se muestra si tiene un prefab asignado
    //     if (achievementPopupPrefab != null)
    //     {
    //         GameObject popup = Instantiate(achievementPopupPrefab, achievementContainer);

    //         // le dice al script del popup qué logro mostrar
    //         AchievementPopup popupScript = popup.GetComponent<AchievementPopup>();

    //         if (popupScript != null)
    //         {
    //             popupScript.DisplayAchievement(achievement);
    //         }

    //     }
    // }

    // para comprobar si un logro en específico ha sido desbloqueado: if (AchievementManager.Instance.IsAchievementUnlocked("achievementId"))
    public bool IsAchievementUnlocked(string achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        return achievement != null && achievement.isUnlocked;
    }

    public Achievement GetAchievement(string achievementId)
    {
        return achievements.Find(a => a.id == achievementId);
    }

    // conseguir todos los logros (la pantalla de logros!!)
    public List<Achievement> GetAllAchievements()
    {
        return achievements;
    }

    // cuántos logros ha desbloqueado 
    public int GetUnlockedCount()
    {
        return achievements.FindAll(a => a.isUnlocked).Count;
    }

    // guardar en base de datos
    private void SaveAchievements()
    {
        Debug.Log("LOGROS GUARDADOS");
    }

    // cargar desde la bbdd
    private void LoadAchievements()
    {
        Debug.Log("CARGANDO DATOS");
    }
}
