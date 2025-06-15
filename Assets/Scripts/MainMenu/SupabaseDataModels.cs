using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Achievement(string id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.isUnlocked = false;
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
    public string refresh_token;
    public SupabaseUser user;
}

// LOADING DATA (player_data)
public class PlayerDataEntry
{
    public string id; // auth.uid()
    public GameSaveData save_data; // el jsonb
    public string last_played; // timestamptz 

    public override string ToString()
    {
        return $"  PlayerDataEntry: ID='{id}', SaveData: {save_data?.ToString() ?? "null"}, LastPlayed='{last_played}'";
    }
}

[System.Serializable]
public class AchievementEntry
{
    public string id { get; set; } // id del usuario
    public string achievement_name { get; set; } // ID del logro
    public bool is_unlocked { get; set; }
    public DateTime unlocked_at { get; set; } // timestamp con timezone
}

// EL CONTENIDO DEL JSONB, la save_data del player_data
[System.Serializable]
public class GameSaveData
{
     public float playerX;
    public float playerY;
    public float playerZ;
    public string currentSceneName;
    public List<string> collectedKeyIDs;

    public GameSaveData()
    {
        playerX = 0f;
        playerY = 0f;
        playerZ = 0f;
        collectedKeyIDs = new List<string>();
        currentSceneName = "";
    }

    public override string ToString()
    {
        return $"GameSaveData: Scene='{currentSceneName}', Pos=({playerX},{playerY}), Keys={collectedKeyIDs?.Count ?? 0}";
    }
}