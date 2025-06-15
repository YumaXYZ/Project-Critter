using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;

public class SupabaseManager : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl = "PROJECT_URL_SUPABASE";
    public string supabaseAnonKey = "PROJECT_ANON_KEY_SUPABASE";

    public static SupabaseManager Instance { get; private set; }

    public string currentUserId { get; private set; }
    public string currentUsername { get; private set; } = "Guest";
    private string currentRefreshToken;

    public GameSaveData currentGameSaveData;

    public List<AchievementEntry> userAchievements = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentUserId = PlayerPrefs.GetString("supabase_user_id", null);
            currentUsername = PlayerPrefs.GetString("current_username", "Guest");
            currentRefreshToken = PlayerPrefs.GetString("supabase_refresh_token", null);

            currentGameSaveData = new GameSaveData();
            Debug.Log("SupabaseManager: Initialized currentGameSaveData.");

            // Intenta cargar la partida si ya hay un usuario logueado desde una sesión anterior
            if (!string.IsNullOrEmpty(currentUserId))
            {
                Debug.Log("SupabaseManager: User ID found, attempting to load game data.");
                LoadGame(); // Llama al nuevo método LoadGame()
            }
            else
            {
                Debug.Log("SupabaseManager: No user logged in, starting with default game data.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetCurrentUserId()
    {
        return currentUserId;
    }

    // AUTH

    public IEnumerator SignUp(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/auth/v1/signup";
        string json = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorResponse = request.downloadHandler.text;
            Debug.LogError("Supabase SignUp Error: " + request.error + " - " + errorResponse);
            onError?.Invoke(errorResponse);
        }
        else
        {
            SupabaseAuthResponse response = JsonUtility.FromJson<SupabaseAuthResponse>(request.downloadHandler.text);

            // Supabase can auto-login after signup if email confirmation is off.
            // If response.user is null, it means email confirmation is required and no auto-login.
            if (response.user != null && !string.IsNullOrEmpty(response.access_token))
            {
                PlayerPrefs.SetString("supabase_access_token", response.access_token);
                PlayerPrefs.SetString("supabase_user_id", response.user.id);
                PlayerPrefs.SetString("supabase_refresh_token", response.refresh_token); // Save refresh token!
                PlayerPrefs.Save();

                currentUserId = response.user.id;
                currentRefreshToken = response.refresh_token;

                // Set initial username 
                currentUsername = email.Split('@')[0]; // Use email prefix as initial default
                PlayerPrefs.SetString("current_username", currentUsername);
                Debug.Log("Sign up successful and user logged in automatically.");
                onSuccess?.Invoke("Registration successful and logged in!");
            }
            else
            {
                Debug.Log("Sign up successful! Please confirm your email and then log in.");
                onSuccess?.Invoke("Registration successful! Please confirm your email.");
            }
        }
    }

    public IEnumerator SignIn(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/auth/v1/token?grant_type=password";
        string json = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorResponse = request.downloadHandler.text;
            Debug.LogError("Supabase SignIn Error: " + request.error + " - " + errorResponse);
            onError?.Invoke(errorResponse);
        }
        else
        {
            SupabaseAuthResponse response = JsonUtility.FromJson<SupabaseAuthResponse>(request.downloadHandler.text);

            PlayerPrefs.SetString("supabase_access_token", response.access_token);
            PlayerPrefs.SetString("supabase_user_id", response.user.id);
            PlayerPrefs.SetString("supabase_refresh_token", response.refresh_token); // <--- SAVE REFRESH TOKEN
            PlayerPrefs.Save(); // Ensure PlayerPrefs are saved immediately

            currentUserId = response.user.id;
            currentRefreshToken = response.refresh_token; // <--- Cache in memory

            // --- Store username from user_metadata (from previous update) ---
            if (response.user != null && response.user.user_metadata != null && !string.IsNullOrEmpty(response.user.user_metadata.username))
            {
                currentUsername = response.user.user_metadata.username;
                PlayerPrefs.SetString("current_username", currentUsername);
            }
            else
            {
                currentUsername = email.Split('@')[0];
                PlayerPrefs.SetString("current_username", currentUsername);
            }

            Debug.Log($"User {email} signed in. ID: {currentUserId}, Username: {currentUsername}");
            onSuccess?.Invoke("Login successful!");
        }
    }

    public IEnumerator SignOut(System.Action onSuccess, System.Action<string> onError)
    {
        string authEndpoint = supabaseUrl + "/auth/v1/logout";
        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");

        // If no access token locally, simply clear local session and treat as logged out
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("Attempted to sign out but no access token found locally. Clearing local session.");
            ClearLocalSessionData(); // Call helper method
            onSuccess?.Invoke();
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(authEndpoint, "POST");
        request.downloadHandler = new DownloadHandlerBuffer(); // Crucial for error text
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorResponse = request.downloadHandler.text;
            Debug.LogError("Supabase SignOut Error: " + request.error + " - " + errorResponse);

            // Even if explicit logout fails (e.g., token expired), we still clear local session data
            // because the user is effectively logged out from client perspective.
            Debug.LogWarning("Logout request failed. Clearing local session anyway.");
            ClearLocalSessionData(); // Call helper method
            onSuccess?.Invoke(); // Call success as local logout is handled
        }
        else
        {
            Debug.Log("Supabase SignOut Success.");
            ClearLocalSessionData(); // Call helper method
            onSuccess?.Invoke();
        }
    }

    private void ClearLocalSessionData()
    {
        PlayerPrefs.DeleteKey("supabase_access_token");
        PlayerPrefs.DeleteKey("supabase_user_id");
        PlayerPrefs.DeleteKey("supabase_refresh_token");
        PlayerPrefs.DeleteKey("current_username");
        PlayerPrefs.Save();

        currentUserId = null;
        currentUsername = "Guest";
        currentRefreshToken = null;
        userAchievements.Clear();
        Debug.Log("Local session data cleared.");
    }

    public IEnumerator RefreshSession(System.Action<bool> onComplete)
    {
        if (string.IsNullOrEmpty(currentRefreshToken))
        {
            Debug.LogWarning("No refresh token available. Cannot refresh session.");
            onComplete?.Invoke(false); // Indicate failure
            yield break;
        }

        string endpoint = supabaseUrl + "/auth/v1/token?grant_type=refresh_token";
        string json = "{\"refresh_token\":\"" + currentRefreshToken + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorResponse = request.downloadHandler.text;
            Debug.LogError($"Supabase RefreshSession Error: {request.error} - {errorResponse}");
            ClearLocalSessionData(); // Clear session if refresh fails (token likely invalid/expired)
            onComplete?.Invoke(false); // Indicate failure
        }
        else
        {
            SupabaseAuthResponse response = JsonUtility.FromJson<SupabaseAuthResponse>(request.downloadHandler.text);

            // Update all local session data with new tokens
            PlayerPrefs.SetString("supabase_access_token", response.access_token);
            PlayerPrefs.SetString("supabase_user_id", response.user.id);
            PlayerPrefs.SetString("supabase_refresh_token", response.refresh_token); // Get new refresh token too!
            PlayerPrefs.Save();

            currentUserId = response.user.id;
            currentRefreshToken = response.refresh_token;

            Debug.Log("Supabase session refreshed successfully.");
            onComplete?.Invoke(true); // Indicate success
        }
    }

    public IEnumerator PerformAuthenticatedRequest(UnityWebRequest originalRequest, System.Action<UnityWebRequest, bool> onComplete)
    {
        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("No access token found for authenticated request. Signalling authentication failure.");
            onComplete?.Invoke(originalRequest, false);
            yield break;
        }

        // --- PRIMER INTENTO ---
        UnityWebRequest currentRequest = new UnityWebRequest(originalRequest.url, originalRequest.method);
        currentRequest.downloadHandler = new DownloadHandlerBuffer();

        // Copiar UploadHandler (si existe)
        if (originalRequest.uploadHandler != null && originalRequest.uploadHandler is UploadHandlerRaw)
        {
            byte[] originalBody = ((UploadHandlerRaw)originalRequest.uploadHandler).data;
            currentRequest.uploadHandler = new UploadHandlerRaw(originalBody);
        }
        // Ejemplo: Si siempre estableces "apikey" en la solicitud original:
        currentRequest.SetRequestHeader("apikey", supabaseAnonKey); // <--- AÑADIR ESTO
        currentRequest.SetRequestHeader("Content-Type", "application/json"); // Ejemplo, también asegúrate que se copia

        // Y luego el encabezado de autorización
        currentRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log("SupabaseManager: Sending initial authenticated request.");
        yield return currentRequest.SendWebRequest(); // Envío del primer intento

        // --- COMPROBACIÓN DE ERRORES Y POSIBLE REINTENTO ---
        bool isTokenError = currentRequest.responseCode == 401 ||
                            (currentRequest.responseCode == 403 &&
                            currentRequest.downloadHandler != null &&
                            (currentRequest.downloadHandler.text.Contains("token is expired") || currentRequest.downloadHandler.text.Contains("invalid JWT")));

        if (currentRequest.result != UnityWebRequest.Result.Success && isTokenError)
        {
            Debug.LogWarning("Access token expired or invalid. Attempting to refresh session...");
            bool refreshSuccess = false;
            yield return StartCoroutine(RefreshSession((success) => refreshSuccess = success));

            if (refreshSuccess)
            {
                Debug.Log("Session refreshed. Retrying original request with new token...");
                string newAccessToken = PlayerPrefs.GetString("supabase_access_token", "");

                UnityWebRequest retryRequest = new UnityWebRequest(originalRequest.url, originalRequest.method);
                retryRequest.downloadHandler = new DownloadHandlerBuffer();
                if (originalRequest.uploadHandler != null && originalRequest.uploadHandler is UploadHandlerRaw)
                {
                    byte[] originalBody = ((UploadHandlerRaw)originalRequest.uploadHandler).data;
                    retryRequest.uploadHandler = new UploadHandlerRaw(originalBody);
                }
                // *** ¡SOLUCIÓN CLAVE! Copiar los encabezados también al reintento ***
                retryRequest.SetRequestHeader("apikey", supabaseAnonKey); // <--- AÑADIR ESTO
                retryRequest.SetRequestHeader("Content-Type", "application/json"); // Ejemplo

                retryRequest.SetRequestHeader("Authorization", "Bearer " + newAccessToken);

                yield return retryRequest.SendWebRequest(); // Envío del reintento

                onComplete?.Invoke(retryRequest, retryRequest.result == UnityWebRequest.Result.Success);
            }
            else
            {
                Debug.LogError("Failed to refresh session. User needs to re-authenticate.");
                ClearLocalSessionData();
                onComplete?.Invoke(currentRequest, false);
            }
        }
        else
        {
            onComplete?.Invoke(currentRequest, currentRequest.result == UnityWebRequest.Result.Success);
        }
    }

    // GUARDAR Y CARGAR PARTIDA
    public void SaveGame()
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("SupabaseManager: Cannot save game data. No user is currently logged in.");
            return;
        }

        if (currentGameSaveData == null) // si no hay datos actuales que guardar
        {
            Debug.LogError("SupabaseManager: No GameSaveData to save! currentGameSaveData is null.");
            return;
        }

        Debug.Log("SupabaseManager: Initiating save/update game data for user: " + currentUserId);

        // Primero mira si hay datos guardados anteriormente
        StartCoroutine(LoadGameDataCoroutine(
            (loadedData) =>
            {
                // Determina si va a ser una actualización/modificación o si es un POST al ser datos desde cero
                // Si no es null, significa que ya había datos antes => PATCH
                // Si es null => POST
                bool isUpdate = (loadedData != null && !string.IsNullOrEmpty(loadedData.currentSceneName));

                StartCoroutine(PerformSaveOrUpdateGameData(isUpdate));
            },
            (error) =>
            {
                // Si falla la carga de datos, significa que no hay datos, así que intentamos hacer un POST
                Debug.LogWarning($"SupabaseManager: Error loading existing save data for user {currentUserId} (or none found): {error}. Attempting to INSERT new data.");
                StartCoroutine(PerformSaveOrUpdateGameData(false)); // False => POST
            }
        ));
    }

    // --- NEW: Coroutine to perform the actual HTTP POST/PATCH request ---
    private IEnumerator PerformSaveOrUpdateGameData(bool isUpdate)
    {
        string endpoint = supabaseUrl + "/rest/v1/player_data";
        UnityWebRequest request;

        // Data to be sent to Supabase. This must match your 'player_data' table structure.
        PlayerDataEntry dataToSend = new PlayerDataEntry
        {
            id = currentUserId,
            save_data = currentGameSaveData, // Use the current in-memory GameSaveData object
            last_played = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        // Serialize the GameSaveData object (nested within PlayerDataEntry) using Newtonsoft.Json
        string jsonPayload = JsonConvert.SerializeObject(dataToSend);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        if (isUpdate)
        {
            // For updating (PATCH), append the ID to the endpoint
            endpoint += "?id=eq." + currentUserId;
            request = new UnityWebRequest(endpoint, "PATCH");
            Debug.Log($"SupabaseManager: Attempting to PATCH (update) game data for user {currentUserId} at {endpoint}");
        }
        else
        {
            // For inserting (POST), send the data to the base endpoint
            request = new UnityWebRequest(endpoint, "POST");
            Debug.Log($"SupabaseManager: Attempting to POST (insert) new game data for user {currentUserId} at {endpoint}");
        }

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Prefer", "return=minimal"); // Optional: return minimal response for efficiency

        UnityWebRequest finalRequest = null;
        bool requestSuccess = false;

        // Use PerformAuthenticatedRequest to handle auth headers
        yield return StartCoroutine(PerformAuthenticatedRequest(request, (req, success) =>
        {
            finalRequest = req;
            requestSuccess = success;
        }));

        if (!requestSuccess)
        {
            string errorMessage = $"Supabase SaveGameData Error: HTTP/1.1 {finalRequest.responseCode} - Raw Response: {finalRequest.downloadHandler.text}";
            Debug.LogError(errorMessage);
            // Optionally, add a callback for failure here if needed
        }
        else
        {
            Debug.Log($"Supabase SaveGameData Success ({(isUpdate ? "PATCH" : "POST")}): {finalRequest.downloadHandler.text}");
            // Optionally, add a callback for success here if needed
        }
    }

    public void LoadGame(System.Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("SupabaseManager: Cannot load game data, no user is currently logged in. Initializing default GameSaveData.");
            currentGameSaveData = new GameSaveData(); // Ensure it's initialized
            if (GameState.Instance != null)
            {
                GameState.Instance.LoadCollectedKeysFromSaveData(); // Applies the default GameSaveData
            }
            onComplete?.Invoke(false);
            return;
        }

        Debug.Log("SupabaseManager: Initiating game data load for user: " + currentUserId);

        StartCoroutine(LoadGameDataCoroutine(
            (loadedGameSaveData) =>
            { // This callback receives a GameSaveData object
                currentGameSaveData = loadedGameSaveData; // Direct assignment
                Debug.Log("SupabaseManager: Game data loaded and assigned successfully!");

                if (GameState.Instance != null)
                {
                    GameState.Instance.ApplyLoadedGameDataToScene(); // This is the method that applies all save data
                    // No need for GameState.Instance.LoadCollectedKeysFromSaveData() if ApplyLoadedGameDataToScene does it all
                    // And no need for GameState.Instance.LoadGameWorldFromSaveData() here,
                    // as ApplyLoadedGameDataToScene is called by OnSceneLoaded, which is triggered by SceneManager.LoadScene
                }
                onComplete?.Invoke(true);
            },
            (error) =>
            {
                Debug.LogError("SupabaseManager: Failed to load game data: " + error);
                currentGameSaveData = new GameSaveData(); // Fallback to default
                if (GameState.Instance != null)
                {
                    GameState.Instance.ApplyLoadedGameDataToScene(); // Apply the default GameSaveData
                }
                onComplete?.Invoke(false);
            }
        ));
    }

    public IEnumerator SaveGameDataCoroutine(string jsonData, System.Action onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/rest/v1/player_data"; // Tu tabla
        string userId = currentUserId;

        if (string.IsNullOrEmpty(userId))
        {
            onError?.Invoke("No user logged in to save data.");
            yield break;
        }

        UnityWebRequest dataRequest = new UnityWebRequest(endpoint, "POST"); // Siempre POST para UPSERT

        string requestBody = "{" +
                            "\"id\":\"" + userId + "\"," +
                            "\"save_data\":" + jsonData + "," +
                            "\"last_played\":\"" + System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"" +
                            "}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        dataRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        dataRequest.downloadHandler = new DownloadHandlerBuffer();
        dataRequest.SetRequestHeader("Content-Type", "application/json");
        dataRequest.SetRequestHeader("apikey", supabaseAnonKey);
        dataRequest.SetRequestHeader("Prefer", "return=representation,resolution=merge-duplicates"); // <-- ¡CLAVE PARA EL UPSERT!

        UnityWebRequest dataFinalRequest = null;
        bool dataSuccess = false;

        // Llamar al wrapper de autenticación
        yield return StartCoroutine(PerformAuthenticatedRequest(dataRequest, (req, success) =>
        {
            dataFinalRequest = req;
            dataSuccess = success;
        }));

        if (!dataSuccess)
        {
            string errorMsg = dataFinalRequest?.error;
            if (string.IsNullOrEmpty(errorMsg)) errorMsg = "Failed to save player data.";
            Debug.LogError("Supabase SaveGameData Error: " + errorMsg + " - Raw Response: " + (dataFinalRequest?.downloadHandler?.text ?? "N/A"));
            onError?.Invoke(errorMsg);
        }
        else
        {
            Debug.Log("Player data saved successfully! Response: " + dataFinalRequest.downloadHandler.text);
            onSuccess?.Invoke();
        }
    }

    public IEnumerator LoadGameDataCoroutine(System.Action<GameSaveData> onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/rest/v1/player_data?id=eq." + currentUserId;
        UnityWebRequest request = UnityWebRequest.Get(endpoint);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);

        UnityWebRequest finalRequest = null;
        bool requestSuccess = false;
        yield return StartCoroutine(PerformAuthenticatedRequest(request, (req, success) =>
        {
            finalRequest = req;
            requestSuccess = success;
        }));

        if (!requestSuccess)
        {
            Debug.LogError("Supabase LoadGameData Error: " + finalRequest.responseCode + " - " + finalRequest.downloadHandler.text);
            onError?.Invoke($"HTTP/1.1 {finalRequest.responseCode} {finalRequest.responseCode} - Raw Response: {finalRequest.downloadHandler.text}");
            onSuccess?.Invoke(new GameSaveData()); // En caso de error de red, pasa un nuevo GameSaveData vacío
        }
        else
        {
            string rawSupabaseJson = finalRequest.downloadHandler.text;
            Debug.Log("Supabase LoadGameData Raw Response: " + rawSupabaseJson);

            GameSaveData loadedGameSaveData = new GameSaveData(); // Inicializar con un default para seguridad

            try
            {
                List<PlayerDataEntry> playerEntries = JsonConvert.DeserializeObject<List<PlayerDataEntry>>(rawSupabaseJson);

                if (playerEntries != null && playerEntries.Count > 0 && playerEntries[0].save_data != null)
                {
                    loadedGameSaveData = playerEntries[0].save_data; // Esto ya es un objeto GameSaveData
                    Debug.Log($"SupabaseManager: Se ha extraído con éxito GameSaveData usando Newtonsoft. Escena: '{loadedGameSaveData.currentSceneName}'");
                }
                else
                {
                    Debug.LogWarning("SupabaseManager: No player data found or save_data is null in response. Initializing default GameSaveData.");
                    // Si no hay datos o son nulos, loadedGameSaveData ya está inicializado como nuevo GameSaveData
                }

                onSuccess?.Invoke(loadedGameSaveData); // Pasa el objeto GameSaveData (deserializado o nuevo)
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al parsear la respuesta de Supabase LoadGameData con Newtonsoft: " + e.Message +
                               "\nJSON crudo era: " + rawSupabaseJson);
                onError?.Invoke("Error al parsear los datos de juego cargados.");
                onSuccess?.Invoke(new GameSaveData()); // En caso de error de deserialización, pasa un nuevo GameSaveData vacío
            }
        }
    }

    // ACHIEVEMENTS

     public IEnumerator LoadUserAchievementsCoroutine(Action<List<AchievementEntry>, string> onComplete = null)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("SupabaseManager: No user logged in. Cannot load achievements.");
            onComplete?.Invoke(new List<AchievementEntry>(), "No user logged in.");
            yield break;
        }

        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("SupabaseManager: No access token found. Cannot load achievements for authenticated user.");
            onComplete?.Invoke(new List<AchievementEntry>(), "No access token available.");
            yield break;
        }

        Debug.Log($"SupabaseManager: Cargando logros para el usuario: {currentUserId}");

        // Define the request outside the try-finally to make it accessible in finally
        UnityWebRequest request = null; 
        try
        {
            request = new UnityWebRequest(
                $"{supabaseUrl}/rest/v1/achievements?id=eq.{currentUserId}&select=*",
                "GET"
            );
            request.downloadHandler = new DownloadHandlerBuffer(); // Ensure downloadHandler is set
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest(); // Send the request directly here

            bool requestSuccess = false;
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"SupabaseManager: Error al cargar logros de usuario: {request.error} - Raw Response: {request.downloadHandler?.text}");
            }
            else
            {
                requestSuccess = true;
            }

            if (!requestSuccess)
            {
                onComplete?.Invoke(new List<AchievementEntry>(), request.error); // Retorna lista vacía y error
            }
            else
            {
                string rawJson = request.downloadHandler.text;
                try
                {
                    List<AchievementEntry> entries = JsonConvert.DeserializeObject<List<AchievementEntry>>(rawJson);
                    if (entries != null)
                    {
                        userAchievements.Clear(); // Limpiar antes de añadir
                        foreach (var entry in entries)
                        {
                            userAchievements.Add(entry); // Añadir el objeto AchievementEntry tal cual
                        }
                    }
                    Debug.Log($"SupabaseManager: Se cargaron {userAchievements.Count} logros para el usuario {currentUserId}.");
                    onComplete?.Invoke(userAchievements, null);

                    // IMPORTANTE: Llamar a la sincronización en AchievementManager después de cargar
                    if (AchievementManager.Instance != null)
                    {
                        // Ensure this method name matches your AchievementManager
                        AchievementManager.Instance.SyncSupabaseAchievements(); 
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"SupabaseManager: Error al parsear logros de usuario: {ex.Message} - JSON: {rawJson}");
                    onComplete?.Invoke(new List<AchievementEntry>(), $"Error parsing user achievements: {ex.Message}");
                }
            }
        }
        finally
        {
            // ALWAYS dispose the UnityWebRequest when done with it
            if (request != null)
            {
                request.Dispose();
            }
        }
    }

    public IEnumerator UnlockAchievementCoroutine(string achievementName, Action onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            onError?.Invoke("No user logged in.");
            yield break;
        }

        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("SupabaseManager: No access token found. Cannot unlock achievement for authenticated user.");
            onError?.Invoke("No access token available.");
            yield break;
        }

        Debug.Log($"SupabaseManager: Desbloqueando logro '{achievementName}' para el usuario {currentUserId}");

        AchievementEntry newEntry = new AchievementEntry
        {
            id = currentUserId, // El user_id en tu tabla
            achievement_name = achievementName,
            is_unlocked = true,
            unlocked_at = DateTime.UtcNow // Siempre usar UTC para consistencia con Supabase
        };

        string jsonBody = JsonConvert.SerializeObject(newEntry);

        // Define the request outside the try-finally
        UnityWebRequest request = null;
        try
        {
            request = new UnityWebRequest(
                $"{supabaseUrl}/rest/v1/achievements",
                "POST"
            );

            request.url += "?on_conflict=id,achievement_name"; // Add on_conflict for UPSERT

            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "resolution=merge-duplicates"); // For UPSERT

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"SupabaseManager: Error al desbloquear logro '{achievementName}': {request.error} - JSON: {jsonBody} - Raw Response: {request.downloadHandler?.text}");
                onError?.Invoke(request.error);
            }
            else
            {
                Debug.Log($"SupabaseManager: Logro '{achievementName}' desbloqueado/actualizado con éxito: {request.downloadHandler?.text}");
                onSuccess?.Invoke();
                // Una vez que se guarda en Supabase, re-sincroniza la lista local de logros
                StartCoroutine(LoadUserAchievementsCoroutine());
            }
        }
        finally
        {
            // ALWAYS dispose the UnityWebRequest when done with it
            if (request != null)
            {
                request.Dispose();
            }
        }
    }
}
