using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class JsonTest : MonoBehaviour
{
    void Start()
    {
        TestJsonDeserialization();
    }

    void TestJsonDeserialization()
    {
        // Use the exact raw response you're getting from Supabase
        string rawSupabaseResponse = "[{\"id\":\"0e9a24f5-7edc-43e2-b0b5-76272a996c25\",\"save_data\":{\"playerX\": 8.84555435180664, \"playerY\": 1.4276002645492554, \"collectedKeyIDs\": [\"houseKey\"], \"currentSceneName\": \"Cemetery\"},\"last_played\":\"2025-06-12T00:26:36.023+00:00\"}]";

         Debug.Log("JsonTest: Starting Newtonsoft.Json deserialization test.");
        Debug.Log("JsonTest: Raw Supabase Response (input for Newtonsoft): " + rawSupabaseResponse);

       try
        {
            // Deserialize directly into a list of PlayerDataEntry.
            // Newtonsoft will automatically deserialize 'save_data' into GameSaveData.
            List<PlayerDataEntry> playerEntries = JsonConvert.DeserializeObject<List<PlayerDataEntry>>(rawSupabaseResponse);

            if (playerEntries != null)
            {
                Debug.Log("JsonTest: List of PlayerDataEntry is NOT null.");
                Debug.Log($"JsonTest: Found {playerEntries.Count} player entries.");

                if (playerEntries.Count > 0)
                {
                    PlayerDataEntry firstEntry = playerEntries[0];

                    Debug.Log("JsonTest: First PlayerDataEntry is NOT null.");
                    Debug.Log("JsonTest: Entry ID: " + firstEntry.id);
                    Debug.Log("JsonTest: Entry Last Played: " + firstEntry.last_played);

                    // --- NO NEED TO DESERIALIZE 'save_data' SEPARATELY ANYMORE ---
                    // It's already an object of type GameSaveData!
                    if (firstEntry.save_data != null)
                    {
                        Debug.Log("JsonTest: GameSaveData accessed directly from PlayerDataEntry!");
                        Debug.Log("JsonTest: PlayerX: " + firstEntry.save_data.playerX);
                        Debug.Log("JsonTest: PlayerY: " + firstEntry.save_data.playerY);
                        Debug.Log("JsonTest: CurrentSceneName: '" + firstEntry.save_data.currentSceneName + "'");
                        Debug.Log("JsonTest: CollectedKeyIDs Count: " + (firstEntry.save_data.collectedKeyIDs?.Count ?? 0));
                        if (firstEntry.save_data.collectedKeyIDs != null && firstEntry.save_data.collectedKeyIDs.Count > 0)
                        {
                            Debug.Log("JsonTest: First Collected Key: " + firstEntry.save_data.collectedKeyIDs[0]);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("JsonTest: save_data object is null for the first entry!");
                    }
                }
                else
                {
                    Debug.LogWarning("JsonTest: Player entries list is empty (Count 0).");
                }
            }
            else
            {
                Debug.LogError("JsonTest: Player entries list is NULL after deserialization!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("JsonTest: An unexpected error occurred during deserialization with Newtonsoft: " + e.Message + "\nStackTrace: " + e.StackTrace);
        }
    }
}
