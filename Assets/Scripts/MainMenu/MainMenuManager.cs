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
        SetMainMenuStatusText($"Welcome, {SupabaseManager.Instance.currentUsername}!"); // mientras carga se mostraría 'Guest' como username

        StartCoroutine(SupabaseManager.Instance.LoadGameData(
            (jsonData) =>
            { // jsonData here is the raw JSON string of GameSaveData
                if (string.IsNullOrEmpty(jsonData) || jsonData == "{}")
                {
                    SetMainMenuStatusText($"Welcome, {SupabaseManager.Instance.currentUsername}!");
                }
                else
                {
                    // Optionally parse the GameSaveData to show specific info, e.g., last played scene
                    // GameSaveData loadedSave = JsonUtility.FromJson<GameSaveData>(jsonData);
                    // mainMenuStatusText.text = $"Welcome back, Operator! Last in: {loadedSave.currentSceneName}";
                    SetMainMenuStatusText($"Welcome back, {SupabaseManager.Instance.currentUsername}!");
                }
            },
            (error) =>
            {
                mainMenuStatusText.text = "Welcome, {SupabaseManager.Instance.currentUsername}! (No saved data detected)";
                Debug.LogWarning("Could not load initial player data: " + error);

                // se el PerformAuth... limpia la sesión por un error al actualizar la sesión (refresh) --> redirige al logIn
                if (string.IsNullOrEmpty(SupabaseManager.Instance.GetCurrentUserId()))
                {
                    Debug.Log("User session expired or invalid. Redirecting to login.");
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
