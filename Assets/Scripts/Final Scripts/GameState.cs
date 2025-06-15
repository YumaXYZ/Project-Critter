using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    // Lista de llaves recogidas (por ID)
    private HashSet<string> collectedKeys = new HashSet<string>();
    private SupabaseManager supabaseManager;
    private Transform playerTransform;
     private CharacterController playerCharacterController;

    private const string MainMenuSceneName = "MainMenu"; 
    // Y el nombre de tu primera escena de juego 
    public string FirstGameSceneName = "Cemetery"; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
            Debug.Log("GameState: Awake - Instance set, DontDestroyOnLoad called. GameObject Name: " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("GameState: Awake - Duplicate detected, destroying: " + gameObject.name);
            Destroy(gameObject);
            // return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento de carga de escena
        Debug.Log("GameState: Subscribed to SceneManager.sceneLoaded.");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Desuscribirse al deshabilitar/destruir
        Debug.Log("GameState: Unsubscribed from SceneManager.sceneLoaded.");
    }

    private void Start()
    {
        Debug.Log("GameState: Start - Attempting to find SupabaseManager.");
        supabaseManager = FindObjectOfType<SupabaseManager>();
        
        if (supabaseManager == null)
        {
            Debug.LogError("GameState: Start - SupabaseManager NOT FOUND. Please ensure it's in the initial scene and properly initialized.");
            enabled = false; // Deshabilitar GameState si no puede encontrar su dependencia crítica.
            return;
        }
        else
        {
            Debug.Log("GameState: Start - SupabaseManager successfully found and assigned.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameState: OnSceneLoaded called for scene: {scene.name}");

        if (scene.name != MainMenuSceneName)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                playerCharacterController = playerObject.GetComponent<CharacterController>(); // <--- GET REFERENCE HERE
                if (playerCharacterController == null)
                {
                    Debug.LogError("GameState: Player has no CharacterController component! Cannot apply loaded position reliably.");
                }
                Debug.Log("GameState: PlayerTransform and CharacterController found and assigned in new scene.");
                
                // Now that the playerTransform and its controller are ready,
                // and the scene is loaded, apply the game data if it was loaded.
                ApplyLoadedGameDataToScene();
            }
            else
            {
                Debug.LogWarning($"GameState: Jugador con tag 'Player' no encontrado en la escena '{scene.name}'. La posición del jugador no se restaurará.");
                playerTransform = null; 
                playerCharacterController = null; // Clear reference too
            }
        }
        else
        {
            playerTransform = null;
            playerCharacterController = null; // Clear reference too
            collectedKeys.Clear();
            Debug.Log("GameState: Returned to MainMenu, playerTransform cleared and collectedKeys reset.");
        }
    }

    public void LoadGameWorldFromSaveData() // <-- THIS IS THE NEW METHOD YOU NEED TO ADD
    {
        Debug.Log("GameState: LoadGameWorldFromSaveData called.");
        if (supabaseManager == null)
        {
            Debug.LogError("GameState: LoadGameWorldFromSaveData - SupabaseManager is null. Cannot load game world.");
            return;
        }
        if (supabaseManager.currentGameSaveData == null)
        {
            Debug.LogError("GameState: LoadGameWorldFromSaveData - currentGameSaveData is null. Cannot load game world.");
            return;
        }

        string savedSceneName = supabaseManager.currentGameSaveData.currentSceneName;
        
        // Default to FirstGameSceneName if no scene is saved or it's the MainMenu
        if (string.IsNullOrEmpty(savedSceneName) || savedSceneName == MainMenuSceneName)
        {
            savedSceneName = FirstGameSceneName;
            Debug.LogWarning($"GameState: Saved scene name is empty or MainMenu. Defaulting to first game scene: {FirstGameSceneName}");
        }

        string currentActiveSceneName = SceneManager.GetActiveScene().name;

        // Load the scene if it's different from the current one
        if (currentActiveSceneName != savedSceneName)
        {
            Debug.Log($"GameState: Loading scene '{savedSceneName}' from save data to continue game.");
            // When the new scene loads, OnSceneLoaded will trigger ApplyLoadedGameDataToScene()
            SceneManager.LoadScene(savedSceneName);
        }
        else
        {
            // If we are already in the correct scene, just apply the data
            Debug.Log($"GameState: Already in saved scene '{savedSceneName}'. Applying loaded data directly.");
            ApplyLoadedGameDataToScene();
        }
    }
    // Este método se llamará después de que SupabaseManager haya cargado los datos.
    // También se llamará cuando una escena se cargue para aplicar los datos.
    public void ApplyLoadedGameDataToScene()
    {
        Debug.Log("GameState: ApplyLoadedGameDataToScene called.");
        if (supabaseManager == null)
        {
            Debug.LogError("GameState: ApplyLoadedGameDataToScene - SupabaseManager is null. Cannot apply game data.");
            return;
        }
        if (supabaseManager.currentGameSaveData == null)
        {
            Debug.LogError("GameState: ApplyLoadedGameDataToScene - currentGameSaveData is null. Cannot apply game data.");
            return;
        }

        // --- 1. Cargar Llaves (already correct) ---
        collectedKeys.Clear();
        if (supabaseManager.currentGameSaveData.collectedKeyIDs != null)
        {
            foreach (string keyID in supabaseManager.currentGameSaveData.collectedKeyIDs)
            {
                collectedKeys.Add(keyID);
            }
            Debug.Log($"GameState: Loaded {collectedKeys.Count} keys from save data into collectedKeys list.");
        }
        else
        {
            Debug.Log("GameState: No collectedKeyIDs found in save data.");
        }

        // --- 2. Cargar Posición del Jugador ---
        string loadedSceneName = supabaseManager.currentGameSaveData.currentSceneName;
        string currentActiveSceneName = SceneManager.GetActiveScene().name;

        // Check if we have a playerTransform and the current scene matches the loaded scene
        if (playerTransform != null && !string.IsNullOrEmpty(loadedSceneName) && loadedSceneName == currentActiveSceneName && loadedSceneName != MainMenuSceneName)
        {
            Vector3 loadedPos = new Vector3(
                supabaseManager.currentGameSaveData.playerX,
                supabaseManager.currentGameSaveData.playerY,
                supabaseManager.currentGameSaveData.playerZ // Make sure playerZ is stored and used
            );

            // --- CRUCIAL: Disable CharacterController before setting position ---
            if (playerCharacterController != null && playerCharacterController.enabled)
            {
                playerCharacterController.enabled = false;
                Debug.Log("GameState: CharacterController temporarily disabled.");
            }

            Debug.Log($"GameState: BEFORE setting player position. Current Player Pos: {playerTransform.position}");
            playerTransform.position = loadedPos;
            Debug.Log($"GameState: AFTER setting player position. New Player Pos: {playerTransform.position}");

            // --- CRUCIAL: Re-enable CharacterController ---
            if (playerCharacterController != null && !playerCharacterController.enabled)
            {
                playerCharacterController.enabled = true;
                Debug.Log("GameState: CharacterController re-enabled.");
            }

            Debug.Log($"GameState: Player position restored to {loadedPos} in scene '{loadedSceneName}'.");
        }
        else if (playerTransform == null && currentActiveSceneName != MainMenuSceneName)
        {
            Debug.LogWarning("GameState: playerTransform is null. Cannot restore player position in this scene.");
        }
        else if (loadedSceneName != currentActiveSceneName)
        {
            Debug.Log($"GameState: Current scene '{currentActiveSceneName}' does not match saved scene '{loadedSceneName}'. Player position not applied directly. Scene change should handle this.");
        }
        else if (string.IsNullOrEmpty(loadedSceneName))
        {
            Debug.Log("GameState: Saved scene name is empty. No position to restore.");
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    public void AddKey(string keyID)
    {
       if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"GameState: Key '{keyID}' added. Total keys: {collectedKeys.Count}");
            SaveGameDataWithKeys(); // Guarda automáticamente al añadir una llave
        }
        else
        {
            Debug.LogWarning($"GameState: Key '{keyID}' already collected.");
        }
    }

   public void LoadCollectedKeysFromSaveData()
    {
        Debug.Log("GameState: LoadCollectedKeysFromSaveData called by SupabaseManager.");
        ApplyLoadedGameDataToScene(); // Simplemente delega en el método de aplicación
    }

    public void SaveGameDataWithKeys()
    {
        Debug.Log("GameState: SaveGameDataWithKeys called.");
        if (supabaseManager == null)
        {
            Debug.LogError("GameState: SaveGameDataWithKeys - SupabaseManager is null. Cannot save.");
            return;
        }
        if (supabaseManager.currentGameSaveData == null)
        {
            Debug.LogError("GameState: SaveGameDataWithKeys - currentGameSaveData is null. Cannot save.");
            return;
        }

        // ¡IMPORTANTE! Solo guardar si NO estamos en la escena del MainMenu.
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == MainMenuSceneName)
        {
            Debug.LogWarning("GameState: Attempting to save from MainMenu. Ignoring position and scene name.");
            supabaseManager.currentGameSaveData.currentSceneName = ""; // No guardes el MainMenu como escena de juego
            supabaseManager.currentGameSaveData.playerX = 0f; 
            supabaseManager.currentGameSaveData.playerY = 0f; 
        }
        else 
        {
            // --- 1. Actualizar las llaves ---
            supabaseManager.currentGameSaveData.collectedKeyIDs = new List<string>(collectedKeys);
            
            // --- 2. Actualizar la posición del jugador ---
            if (playerTransform != null)
            {
                supabaseManager.currentGameSaveData.playerX = playerTransform.position.x;
                supabaseManager.currentGameSaveData.playerY = playerTransform.position.y;
                supabaseManager.currentGameSaveData.playerZ = playerTransform.position.z;
                Debug.Log($"GameState: Player position ({playerTransform.position.x}, {playerTransform.position.y}, {playerTransform.position.z}) ready for save.");
            }
            else
            {
                Debug.LogWarning("GameState: playerTransform is null. Player position will NOT be saved.");
            }

            // --- 3. Actualizar el nombre de la escena ---
            supabaseManager.currentGameSaveData.currentSceneName = currentScene; 
            Debug.Log($"GameState: Current scene name ('{currentScene}') ready for save.");
        }

        // --- 4. Solicitar a SupabaseManager que realice la operación de guardado real ---
        supabaseManager.SaveGame(); 
        Debug.Log("GameState: Save request sent to SupabaseManager.");
    }

    public void LoadScene(string sceneName, bool saveBeforeLoading = false)
    {
        if (saveBeforeLoading)
        {
            Debug.Log($"GameState: Guardando partida antes de cargar la escena '{sceneName}'.");
            SaveGameDataWithKeys(); // Llama al guardado completo antes de cambiar de escena.
        }

        SceneManager.LoadScene(sceneName);
        Debug.Log($"GameState: Cargando escena: {sceneName}");
    }

    public void LoadGameFromSave()
    {
         Debug.Log("GameState: LoadGameFromSave called (e.g., by 'Continue' button).");
        supabaseManager.LoadGame((success) => {
            if (success)
            {
                Debug.Log("GameState: SupabaseManager.LoadGame completed successfully.");
                // Ahora, decide qué escena cargar. La aplicación de los datos ocurrirá
                // en OnSceneLoaded -> ApplyLoadedGameDataToScene().
                if (supabaseManager.currentGameSaveData != null && 
                    !string.IsNullOrEmpty(supabaseManager.currentGameSaveData.currentSceneName) &&
                    supabaseManager.currentGameSaveData.currentSceneName != MainMenuSceneName)
                {
                    string sceneToLoad = supabaseManager.currentGameSaveData.currentSceneName;
                    
                    if (SceneManager.GetActiveScene().name != sceneToLoad)
                    {
                        Debug.Log($"GameState: Loading saved scene: {sceneToLoad}");
                        SceneManager.LoadScene(sceneToLoad); 
                    }
                    else
                    {
                        Debug.Log($"GameState: Already in saved scene '{sceneToLoad}'. Applying loaded data.");
                        // Si ya estamos en la escena, ApplyLoadedGameDataToScene ya debería haber sido llamado
                        // por OnSceneLoaded. Llamarlo de nuevo aquí asegura que los datos más frescos se aplican.
                        ApplyLoadedGameDataToScene(); 
                    }
                }
                else
                {
                    Debug.LogWarning("GameState: No valid saved scene found (empty save or MainMenu). Loading default first game scene.");
                    SceneManager.LoadScene(FirstGameSceneName);
                }
            }
            else
            {
                Debug.LogError("GameState: Failed to load game data from Supabase. Loading default first game scene.");
                SceneManager.LoadScene(FirstGameSceneName);
            }
        });
    }
    
    public void ManualSaveGame()
    {
        Debug.Log("GameState: Guardado manual de la partida iniciado.");
        SaveGameDataWithKeys(); // Llama al método centralizado de guardado.
    }
}
