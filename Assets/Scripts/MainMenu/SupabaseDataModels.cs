using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    // atributos
    public string id;
    public string title;
    public string description;
    public string icon; // icono?
    public bool isUnlocked; // si lo ha desbloqueado antes o no
    public DateTime unlockedDate; // fecha cuando desbloqueó el logro

    // constructor
    public Achievement(string id, string title, string description, string icon = null)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.icon = icon;
        this.isUnlocked = false; // siempre empieza como bloqueado
        this.unlockedDate = DateTime.MinValue;
    }

    // IMPORTANT: JsonUtility has issues with DateTime.
    // For saving/loading, we convert DateTime to a string.
    public string GetUnlockedDateString()
    {
        return unlockedDate.ToString("o"); // "o" for round-trip format
    }

    public void SetUnlockedDateFromString(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            unlockedDate = date;
        }
        else
        {
            unlockedDate = DateTime.MinValue; // Fallback for parsing errors
        }
    }

    // Helper method to load the Sprite at runtime
    // This assumes your sprites are in a Resources folder, e.g., Assets/Resources/AchievementIcons/
    public Sprite GetIconSprite()
    {
        if (string.IsNullOrEmpty(icon)) return null;
        return Resources.Load<Sprite>("AchievementIcons/" + icon); // Adjust path as needed
    }
}

[System.Serializable]
public class UserMetadata
{
    public string username; // el display name en la tabla auth de supabase
}

[System.Serializable]
public class SupabaseUser
{
    public string id;
    public string email;
    public UserMetadata user_metadata;
}

[System.Serializable]
public class SupabaseAuthResponse
{
    public string access_token;
    public string token_type;
    public int expires_in;
    public string refresh_token;
    public SupabaseUser user;
}

// LOADING DATA (player_data)
public class PlayerDataEntry
{
    public string id; // auth.uid()
    public string save_data; // el jsonb
    public string last_played; // timestamptz 
}

[System.Serializable]
public class PlayerDataResponseWrapper // For deserializing the array response
{
    public PlayerDataEntry[] data; // JsonUtility requires this wrapper for top-level arrays
}

[System.Serializable]
public class AchievementEntry
{
    public string id; // userId
    public string achievement_name;
    public bool is_unlocked;
    public string unlocked_at; // timestamptz 
}

[System.Serializable]
public class AchievementsResponseWrapper
{
    public AchievementEntry[] data;
}

// OBJETO ÚNICO EN INVENTARIO
[System.Serializable]
public class InventoryItem
{
    public string itemId;    // ejemplo: 'cabinKey'
    
    public InventoryItem(string id)
    {
        itemId = id;
    }
}

// EL CONTENIDO DEL JSONB, la save_data del player_data
[System.Serializable]
public class GameSaveData
{
    // posición del personaje
    public float playerX;
    public float playerY;
    // escena 
    public string currentSceneName; // ejemplo: 'Cemetery'
    public List<InventoryItem> inventory; // lista de items en el inventario
                                          // public int score
                                          // public int timePlayed
    public GameSaveData()
    {
        // se inicializa para evitar NullReferenceException si no hay items
        inventory = new List<InventoryItem>();
    }
}