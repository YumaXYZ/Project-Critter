using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject menuPanel; // Canvas > Menu
    public GameObject achievementPanel;
    public GameObject optionsPanel;
    public GameObject optionsManager;

    // para algún mensaje en el menuPanel en plan Bienvenido, ___! 
    [Header("Main Menu Status Text")]
    public TMP_Text mainMenuStatusText;

    [Header("Login Panel Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text loginMessageText;

    void Awake()
    {
        if (SupabaseManager.Instance == null)
        {
            Debug.LogError("SupabaseManager not found! :("); // tiene que estar en la escena y puesto como DontDestroyOnLoad
        }
        
        // if (AchievementManager.Instance == null)
        // {
        //     Debug.LogError("AchievementManager not found! :(");
        // }
    }

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);

        string accessToken = PlayerPrefs.GetString("supabase_access_token", "");
        string userId = PlayerPrefs.GetString("supabase_user_id", "");

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(userId))
        {
            loginMessageText.text = "Existing session detected... Loading...";
            ShowPanel(menuPanel);
            LoadPlayerInitialData();
        }
        else
        {
            ShowPanel(loginPanel);
        }
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // Desactivar todos los paneles principales
        if (loginPanel != null) loginPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);
        if (optionsManager != null && optionsManager.gameObject != null)
        {
            optionsManager.gameObject.SetActive(false); // Desactivar el panel de opciones
        }

        // Activar solo el panel deseado
        if (panelToShow == loginPanel)
        {
            menuPanel.SetActive(true); // El login es un popup sobre el menú
            loginPanel.SetActive(true);
        }
        else if (panelToShow == menuPanel)
        {
            menuPanel.SetActive(true);
        }
        else if (panelToShow == achievementPanel)
        {
            achievementPanel.SetActive(true);
        }
        else if (panelToShow == optionsPanel)
        {
            // Ocultar el menú principal al mostrar opciones
            menuPanel.SetActive(false); 
            if (optionsManager != null) optionsManager.gameObject.SetActive(true); // Activar el GameObject del OptionsManager
        }
        else
        {
            loginPanel.SetActive(false);
            if (panelToShow != null)
            {
                panelToShow.SetActive(true);
            }
        }
        loginMessageText.text = "";
    }

    private void SetMainMenuStatusText(string message)
    {
        if (mainMenuStatusText != null)
        {
            mainMenuStatusText.text = message;
        }
    }

    void OnLoginButtonClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginMessageText.text = "Enter your email and password.";
            return;
        }

        loginMessageText.text = "Accessing database...";
        StartCoroutine(SupabaseManager.Instance.SignIn(email, password,
            (msg) =>
            {
                loginMessageText.text = "Login successful!";
                ShowPanel(menuPanel);
                LoadPlayerInitialData();
            },
            (error) =>
            {
                loginMessageText.text = "Login failed: " + error;
                Debug.LogError("Login failed: " + error);
            }
        ));
    }

    void OnRegisterButtonClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginMessageText.text = "Registration requires both fields.";
            return;
        }

        loginMessageText.text = "Creating new user profile...";
        StartCoroutine(SupabaseManager.Instance.SignUp(email, password,
            (msg) =>
            {
                loginMessageText.text = msg + "\nAttempting immediate login...";
                // Si el registro es exitoso, se intenta loggear automáticamente
                StartCoroutine(SupabaseManager.Instance.SignIn(email, password,
                    (loginMsg) =>
                    {
                        loginMessageText.text = "Profile created. Welcome!";
                        ShowPanel(menuPanel);
                        LoadPlayerInitialData();
                    },
                    (loginError) =>
                    {
                        loginMessageText.text = "Login after registration failed: " + loginError;
                        Debug.LogError("Login after registration failed: " + loginError);
                    }
                ));
            },
            (error) =>
            {
                loginMessageText.text = "Registration failed: " + error;
                Debug.LogError("Registration failed: " + error);
            }
        ));
    }

    void LoadPlayerInitialData()
    {
        SetMainMenuStatusText($"Welcome, {SupabaseManager.Instance.currentUsername}!");

        StartCoroutine(SupabaseManager.Instance.LoadGameDataCoroutine(
            (loadedGameSaveData) => // <--- ¡CAMBIO AQUÍ! Ahora el parámetro es GameSaveData
            {
                // Ahora, loadedGameSaveData es un objeto GameSaveData
                // Puedes comprobar si es null o si es el objeto por defecto/vacío que se pasa en caso de no encontrar datos.
                
                // Si SupabaseManager.LoadGameDataCoroutine pasa un 'new GameSaveData()' cuando no hay guardado,
                // entonces 'loadedGameSaveData' no será null. Puedes comprobar su contenido.
                if (loadedGameSaveData == null || loadedGameSaveData.currentSceneName == "") // Comprobar si es un guardado "vacío" o nulo
                {
                    SetMainMenuStatusText($"Welcome, {SupabaseManager.Instance.currentUsername}! (No saved data found or empty save)");
                    Debug.Log("MainMenuManager: No valid saved game data found. User logs in for the first time or save is empty.");
                }
                else
                {
                    // Los datos de guardado se han cargado correctamente.
                    // Accede a sus propiedades directamente.
                    SetMainMenuStatusText($"Welcome back, {SupabaseManager.Instance.currentUsername}! Last scene: {loadedGameSaveData.currentSceneName}");
                    Debug.Log($"MainMenuManager: Loaded game data. Last scene: {loadedGameSaveData.currentSceneName}");
                    
                    // Ahora, si el GameState necesita esta data para cambiar de escena,
                    // probablemente ya lo gestiona llamando a GameState.Instance.LoadCollectedKeysFromSaveData()
                    // desde SupabaseManager.LoadGame, que ya tiene acceso a SupabaseManager.Instance.currentGameSaveData.
                    // No necesitas hacer JsonUtility.FromJson aquí, ya tienes el objeto.
                }
            },
            (error) =>
            {
                mainMenuStatusText.text = $"Welcome, {SupabaseManager.Instance.currentUsername}! (Error loading saved data)";
                Debug.LogWarning("MainMenuManager: Could not load initial player data: " + error);

                if (string.IsNullOrEmpty(SupabaseManager.Instance.GetCurrentUserId()))
                {
                    Debug.Log("MainMenuManager: User session expired or invalid. Redirecting to login.");
                    ShowPanel(loginPanel);
                    SetMainMenuStatusText("Your session has expired. Please log in.");
                }
            }
        ));
    }
     public void LogoutAndShowLogin()
    {
        mainMenuStatusText.text = "Disconnecting session...";
        StartCoroutine(SupabaseManager.Instance.SignOut(
            () => {
                mainMenuStatusText.text = "Session ended. Please log in.";
                ShowPanel(loginPanel); // Go back to the login panel
            },
            (error) => {
                mainMenuStatusText.text = "Logout failed: " + error;
                Debug.LogError("Logout error: " + error);
            }
        ));
    }
}
