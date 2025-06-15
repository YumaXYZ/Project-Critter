// AchievementManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("Achievement Settings")]
    public GameObject achievementPopupPrefab; // el popup prefab aquí en el inspector (Opcional)
    public Transform achievementContainer; // donde aparecen los logros (canvas), para el popup (Opcional)

    [Header("Achievement List")]
    // Esta lista se llenará con todas las definiciones de logros del juego
    public List<Achievement> achievements = new();

    // Evento para que otros scripts puedan escucharlo cuando un logro se desbloquea
    public static event Action<Achievement> OnAchievementUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAchievements(); // Define todos los logros posibles del juego

            // El SupabaseManager es quien carga los logros de usuario al inicio de sesión
            // o al restaurar la sesión. Cuando SupabaseManager termina de cargar,
            // llamará a SincronizarLogrosConSupabase() en este manager.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAchievements()
    {
        achievements.Clear();

        // --- DEFINE AQUÍ TODOS LOS LOGROS DE TU JUEGO ---
        // El 'id' debe ser único y coincidir con 'achievement_name' en tu tabla 'user_achievements' de Supabase.
        // El 'iconPath' es opcional, si usas Resources.Load para cargar sprites por nombre/ruta.
        achievements.Add(new Achievement(
            "hello_zacarias",
            "Hello, Zacarías!",
            "Meet Zacarías for the first time"
        ));
        
        achievements.Add(new Achievement(
            "cabin",
            "It's cozy",
            "Open the house with the key you found"
        ));
    }

    /// <summary>
    /// Sincroniza el estado de los logros locales (definiciones) con los datos cargados desde SupabaseManager.
    /// Este método es invocado por SupabaseManager cuando termina de cargar los logros del usuario.
    /// </summary>
    public void SyncSupabaseAchievements()
    {
        // Si no hay SupabaseManager o usuario logueado, asumimos que todos los logros están bloqueados.
        if (SupabaseManager.Instance == null || string.IsNullOrEmpty(SupabaseManager.Instance.currentUserId))
        {
            Debug.LogWarning("AchievementManager: No SupabaseManager o usuario logueado para sincronizar logros. Todos los logros se consideran no desbloqueados.");
            foreach (var ach in achievements)
            {
                ach.isUnlocked = false;
            }
            return;
        }

        Debug.Log("AchievementManager: Sincronizando logros locales con datos de Supabase...");
        
        // Obtenemos la lista de logros del usuario que SupabaseManager ya cargó de la DB.
        List<AchievementEntry> loadedUserAchievements = SupabaseManager.Instance.userAchievements;

        // Itera sobre las definiciones de logros locales
        foreach (var localAch in achievements)
        {
            // Busca si este logro está en la lista de logros desbloqueados por el usuario
            AchievementEntry userAchState = loadedUserAchievements.Find(a => a.id == localAch.id);

            if (userAchState != null && userAchState.is_unlocked)
            {
                // Si el logro está en la lista de usuario y está desbloqueado, actualiza el estado local
                localAch.isUnlocked = true;
                Debug.Log($"AchievementManager: Logro '{localAch.title}' sincronizado como desbloqueado.");
            }
            else
            {
                // Si no se encontró o no está desbloqueado por el usuario, asegúrate de que esté marcado como bloqueado localmente.
                localAch.isUnlocked = false;
            }
        }
        Debug.Log("AchievementManager: Sincronización de logros completada.");
    }

    // Se puede llamar desde cualquier script del juego para desbloquear un logro
    // Ejemplo: AchievementManager.Instance.UnlockAchievement("hello_zacarias");
    public void UnlockAchievement(string achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);

        if (achievement != null && !achievement.isUnlocked)
        {
            // Marca el logro como desbloqueado localmente inmediatamente
            achievement.isUnlocked = true;
            achievement.unlockedDate = DateTime.Now;

            Debug.Log($"Achievement unlocked locally -> {achievement.title}");

            // Notifica a otros scripts (ej. para un popup)
            OnAchievementUnlocked?.Invoke(achievement);

            // (Opcional) Muestra el popup del logro
            // ShowAchievementPopup(achievement);

            // --- LLAMADA A SUPABASE PARA GUARDAR EL LOGRO ---
            if (SupabaseManager.Instance != null && !string.IsNullOrEmpty(SupabaseManager.Instance.currentUserId))
            {
                Debug.Log($"AchievementManager: Intentando guardar logro '{achievement.id}' en Supabase.");
                StartCoroutine(SupabaseManager.Instance.UnlockAchievementCoroutine(
                    achievement.id,
                    () => Debug.Log($"Logro '{achievement.id}' guardado con éxito en Supabase."),
                    (error) => Debug.LogError($"Fallo al guardar logro '{achievement.id}' en Supabase: {error}")
                ));
            }
            else
            {
                Debug.LogWarning($"AchievementManager: No user logged in. Logro '{achievement.id}' desbloqueado localmente, pero NO se guardó en Supabase.");
            }
        }
        else if (achievement != null && achievement.isUnlocked)
        {
            Debug.Log($"Achievement '{achievement.title}' is already unlocked. No action needed.");
        }
        else
        {
            Debug.LogWarning($"AchievementManager: Achievement with ID '{achievementId}' not found in local definitions.");
        }
    }

    // Puedes implementar este método si tienes un prefab de popup para logros individuales
    /*
    private void ShowAchievementPopup(Achievement achievement)
    {
        if (achievementPopupPrefab != null && achievementContainer != null)
        {
            GameObject popup = Instantiate(achievementPopupPrefab, achievementContainer);
            AchievementPopup popupScript = popup.GetComponent<AchievementPopup>(); // Asume que tienes un script AchievementPopup en tu prefab
            if (popupScript != null)
            {
                // popupScript.DisplayAchievement(achievement); // Implementa este método en AchievementPopup
            }
            else
            {
                Debug.LogWarning("AchievementPopup script not found on achievementPopupPrefab.");
            }
        }
        else
        {
            Debug.LogWarning("Achievement popup prefab or container not assigned in AchievementManager.");
        }
    }
    */

    // Para comprobar si un logro en específico ha sido desbloqueado:
    // Ejemplo: if (AchievementManager.Instance.IsAchievementUnlocked("hello_zacarias"))
    public bool IsAchievementUnlocked(string achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        return achievement != null && achievement.isUnlocked;
    }

    // Para obtener un objeto Achievement específico
    public Achievement GetAchievement(string achievementId)
    {
        return achievements.Find(a => a.id == achievementId);
    }

    // Para obtener la lista completa de todos los logros (para la pantalla de logros)
    public List<Achievement> GetAllAchievements()
    {
        return achievements;
    }

    // Para obtener el número de logros que el usuario ha desbloqueado
    public int GetUnlockedCount()
    {
        return achievements.FindAll(a => a.isUnlocked).Count;
    }
}