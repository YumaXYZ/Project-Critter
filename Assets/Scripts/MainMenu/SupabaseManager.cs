using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

public class SupabaseManager : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl = "PROJECT_URL_SUPABASE";
    public string supabaseAnonKey = "PROJECT_ANON_KEY_SUPABASE";

    public static SupabaseManager Instance { get; private set; }

    public string currentUserId { get; private set; }
    public string currentUsername { get; private set; } = "Guest";
    private string currentRefreshToken;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentUserId = PlayerPrefs.GetString("supabase_user_id", null);
            currentUsername = PlayerPrefs.GetString("current_username", "Guest");
            currentRefreshToken = PlayerPrefs.GetString("supabase_refresh_token", null);
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

                // Set initial username (from previous update)
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
        PlayerPrefs.Save(); // Ensure changes are saved

        currentUserId = null;
        currentUsername = "Guest";
        currentRefreshToken = null;
        Debug.Log("Local session data cleared.");
    }

    public IEnumerator UpdateUserMetadata(string newUsername, System.Action onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/auth/v1/user";
        // Create a temporary object to hold the metadata
        UserMetadata metadataToUpdate = new UserMetadata { username = newUsername };
        // Create the full JSON payload with the "data" field
        string jsonPayload = "{\"data\":" + JsonUtility.ToJson(metadataToUpdate) + "}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(endpoint, "PUT"); // Use PUT to update user data
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);

         UnityWebRequest finalRequest = null;
        bool requestSuccess = false; // <--- ADD THIS BOOLEAN TO CAPTURE THE SUCCESS STATE
        yield return StartCoroutine(PerformAuthenticatedRequest(request, (req, success) => { // <--- ADD 'success' HERE
            finalRequest = req;
            requestSuccess = success; // <--- ASSIGN THE SUCCESS STATE
        }));

        // Now, check the 'requestSuccess' boolean instead of finalRequest.result directly
        if (!requestSuccess) // Use the new success flag!
        {
            // PerformAuthenticatedRequest already logs specific errors.
            // Here, we provide a more general error message or propagate the specific one.
            string errorResponse = finalRequest?.error;
            if (string.IsNullOrEmpty(errorResponse)) // Fallback for cases where 'error' might be null
            {
                errorResponse = "Request failed or session invalid. Please log in again.";
            }

            Debug.LogError("Supabase UpdateUserMetadata Error: " + errorResponse + " - Raw Response: " + (finalRequest?.downloadHandler?.text ?? "N/A"));
            onError?.Invoke(errorResponse);

            // You might want to force a re-login if the token was cleared by PerformAuthenticatedRequest
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("supabase_access_token", "")))
            {
                Debug.Log("Session invalid, returning to login screen.");
                // MainMenuManager.Instance.ForceLoginScreen(); // (Implement this in MainMenuManager)
            }
        }
        else
        {
            // Update cached username
            currentUsername = newUsername; // Use 'CurrentUsername' (public property)
            PlayerPrefs.SetString("current_username", currentUsername);
            PlayerPrefs.Save();
            Debug.Log($"User metadata updated successfully. New username: {newUsername}");
            onSuccess?.Invoke();
        }
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

            // Update username in case it was modified server-side or if it's the first time
            if (response.user != null && response.user.user_metadata != null && !string.IsNullOrEmpty(response.user.user_metadata.username))
            {
                currentUsername = response.user.user_metadata.username;
                PlayerPrefs.SetString("current_username", currentUsername);
            }
            Debug.Log("Supabase session refreshed successfully.");
            onComplete?.Invoke(true); // Indicate success
        }
    }

    // --- NEW: Helper for Authenticated Requests with Refresh Logic ---
    // This method will wrap your actual authenticated API calls
    public IEnumerator PerformAuthenticatedRequest(UnityWebRequest request, System.Action<UnityWebRequest, bool> onComplete)
    {
        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");

        // If no access token is available, indicate failure directly via callback
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("No access token found for authenticated request. Signalling authentication failure.");
            // Pass the original request (which hasn't been sent), and false for success
            // Callers will need to check request.result and request.error for details if needed,
            // but the 'false' in the callback is the primary signal.
            onComplete?.Invoke(request, false);
            yield break; // Exit the coroutine immediately
        }

        // Only set Authorization header if we have a token (which we now know we do)
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest(); // First attempt to send the actual web request

        // Check for token-related errors (401 Unauthorized or 403 Forbidden with specific messages)
        bool isTokenError = request.responseCode == 401 ||
                            (request.responseCode == 403 &&
                             request.downloadHandler != null && // Check if downloadHandler exists before accessing text
                             (request.downloadHandler.text.Contains("token is expired") || request.downloadHandler.text.Contains("invalid JWT")));

        if (request.result != UnityWebRequest.Result.Success && isTokenError)
        {
            Debug.LogWarning("Access token expired or invalid. Attempting to refresh session...");
            bool refreshSuccess = false;
            yield return StartCoroutine(RefreshSession((success) => refreshSuccess = success));

            if (refreshSuccess)
            {
                Debug.Log("Session refreshed. Retrying original request...");
                string newAccessToken = PlayerPrefs.GetString("supabase_access_token", "");
                request.SetRequestHeader("Authorization", "Bearer " + newAccessToken);

                yield return request.SendWebRequest(); // Retry the request with the new token
            }
            else
            {
                Debug.LogError("Failed to refresh session. User needs to re-authenticate.");
                ClearLocalSessionData(); // If refresh token also failed, clear all data
                // Pass the original request (which now reflects the failed retry) and false for success
                onComplete?.Invoke(request, false); // Signal ultimate failure
                yield break; // Exit if refresh failed
            }
        }
        // If initial request was successful, or if refresh was successful and retry was successful:
        onComplete?.Invoke(request, request.result == UnityWebRequest.Result.Success);
    }

    // GUARDAR Y CARGAR PARTIDA
    public IEnumerator SaveGameData(string jsonData, System.Action onSuccess, System.Action<string> onError)
    {
        // currentUserId ya se gestiona internamente y se comprueba en PerformAuthenticatedRequest
        
        string endpoint = supabaseUrl + "/rest/v1/player_data"; // Tu tabla
        string userId = currentUserId;

        if (string.IsNullOrEmpty(userId))
        {
            onError?.Invoke("No user logged in to save data.");
            yield break;
        }

        // Primero, verifica si ya existen datos para este user_id
        string selectEndpoint = endpoint + "?user_id=eq." + userId;
        UnityWebRequest selectRequest = UnityWebRequest.Get(selectEndpoint);
        selectRequest.downloadHandler = new DownloadHandlerBuffer();
        selectRequest.SetRequestHeader("apikey", supabaseAnonKey);

        UnityWebRequest selectFinalRequest = null;
        bool selectSuccess = false;
        yield return StartCoroutine(PerformAuthenticatedRequest(selectRequest, (req, success) => {
            selectFinalRequest = req;
            selectSuccess = success;
        }));

        if (!selectSuccess)
        {
            string errorMsg = selectFinalRequest?.error;
            if (string.IsNullOrEmpty(errorMsg)) errorMsg = "Failed to check for existing player data.";
            Debug.LogError("Supabase SaveGameData (check) Error: " + errorMsg + " - Raw Response: " + (selectFinalRequest?.downloadHandler?.text ?? "N/A"));
            onError?.Invoke(errorMsg);
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("supabase_access_token", ""))) Debug.Log("Session invalid, returning to login screen.");
            yield break;
        }

        PlayerDataResponseWrapper existingData = JsonUtility.FromJson<PlayerDataResponseWrapper>("{\"data\":" + selectFinalRequest.downloadHandler.text + "}");

        UnityWebRequest dataRequest;
        string requestBody;

        if (existingData.data != null && existingData.data.Length > 0)
        {
            // El usuario ya tiene datos, actualizarlos (PATCH)
            // Necesitamos la 'id' del registro existente para actualizarlo, si tu PK es 'id'
            // Si user_id es tu PK, entonces PATCH con user_id=eq.
            string recordId = existingData.data[0].id; // Asumiendo que 'id' es la PK de la tabla player_data
            dataRequest = new UnityWebRequest(endpoint + "?id=eq." + recordId, "PATCH");
            requestBody = "{\"save_data\":\"" + jsonData + "\"}";
            Debug.Log($"Updating existing player data for user: {userId}, record ID: {recordId}");
        }
        else
        {
            // El usuario no tiene datos, insertarlos (POST)
            dataRequest = new UnityWebRequest(endpoint, "POST");
            requestBody = "{\"user_id\":\"" + userId + "\",\"save_data\":\"" + jsonData + "\"}"; // Asegúrate que user_id sea un campo en tu tabla
            Debug.Log($"Inserting new player data for user: {userId}");
        }

        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        dataRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        dataRequest.downloadHandler = new DownloadHandlerBuffer();
        dataRequest.SetRequestHeader("Content-Type", "application/json");
        dataRequest.SetRequestHeader("apikey", supabaseAnonKey);
        dataRequest.SetRequestHeader("Prefer", "return=representation"); // Obtener la fila insertada/actualizada
        // dataRequest.SetRequestHeader("Prefer", "resolution=merge-duplicates"); // Solo para PATCH si tu PK es el user_id y quieres insertar/actualizar
                                                                               // Sin embargo, si buscas por user_id y actualizas por ID (PK), PATCH es mejor.


        UnityWebRequest dataFinalRequest = null;
        bool dataSuccess = false;
        yield return StartCoroutine(PerformAuthenticatedRequest(dataRequest, (req, success) => {
            dataFinalRequest = req;
            dataSuccess = success;
        }));

        if (!dataSuccess)
        {
            string errorMsg = dataFinalRequest?.error;
            if (string.IsNullOrEmpty(errorMsg)) errorMsg = "Failed to save player data.";
            Debug.LogError("Supabase SaveGameData Error: " + errorMsg + " - Raw Response: " + (dataFinalRequest?.downloadHandler?.text ?? "N/A"));
            onError?.Invoke(errorMsg);
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("supabase_access_token", ""))) Debug.Log("Session invalid, returning to login screen.");
        }
        else
        {
            Debug.Log("Player data saved successfully!");
            onSuccess?.Invoke();
        }
    }
    
    public IEnumerator LoadGameData(System.Action<string> onSuccess, System.Action<string> onError)
    {
        string endpoint = supabaseUrl + "/rest/v1/player_data?id=eq." + currentUserId; // id es la columna de la tabla player_data
        UnityWebRequest request = UnityWebRequest.Get(endpoint);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);

        // --- Usa el nuevo wrapper de petición autenticada ---
        UnityWebRequest finalRequest = null;
        bool requestSuccess = false; // Nueva bandera booleana para capturar el éxito
        yield return StartCoroutine(PerformAuthenticatedRequest(request, (req, success) =>
        {
            finalRequest = req;
            requestSuccess = success;
        }));

        // NOTA: Si requestSuccess es false, PerformAuthenticatedRequest ya habrá registrado un error
        // y potencialmente limpiado los datos de la sesión local si el refresco falló.
        // Aquí solo necesitamos manejar el resultado final.
        if (!requestSuccess) // ¡Comprueba la nueva bandera de éxito!
        {
            string errorMsg = finalRequest?.error; // Obtiene el error del objeto request
            if (string.IsNullOrEmpty(errorMsg)) // Fallback si el error no está establecido o request es null
            {
                // Esto podría ocurrir si PerformAuthenticatedRequest limpió la sesión
                // o si hay un problema de conexión general no manejado.
                errorMsg = "Request failed or session invalid. Please log in again.";
            }
            Debug.LogError("Supabase LoadGameData Error: " + errorMsg + " - Raw Response: " + (finalRequest?.downloadHandler?.text ?? "N/A"));
            onError?.Invoke(errorMsg);

            // Si la sesión se limpió por PerformAuthenticatedRequest debido a un fallo de refresco,
            // MainMenuManager debería ser notificado para redirigir al login.
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("supabase_access_token", "")))
            {
                // Aquí podrías llamar a una función en MainMenuManager para forzar la pantalla de login.
                // Ejemplo: MainMenuManager.Instance.ForceLoginScreen(); (tendrías que implementar esto)
                Debug.Log("Sesión inválida, volviendo a la pantalla de inicio de sesión.");
            }
        }
        else
        {
            Debug.Log("Supabase LoadGameData Raw Response: " + finalRequest.downloadHandler.text);
            try
            {
                // JsonUtility no puede deserealizar un array directamente, así que lo envolvemos
                string jsonResponse = "{\"data\":" + finalRequest.downloadHandler.text + "}";
                PlayerDataResponseWrapper responseWrapper = JsonUtility.FromJson<PlayerDataResponseWrapper>(jsonResponse);

                if (responseWrapper != null && responseWrapper.data != null && responseWrapper.data.Length > 0)
                {
                    onSuccess?.Invoke(responseWrapper.data[0].save_data);
                }
                else
                {
                    onSuccess?.Invoke("{}"); // Devuelve un objeto JSON vacío si no hay datos
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing Supabase LoadGameData response: " + e.Message);
                onError?.Invoke("Error parsing loaded game data.");
            }
        }
    }
}
