using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas; // canvas de pausa, arrastrarlo al inspector
    public GameObject optionsPanel;

    private bool isPaused = false;

    public string mainMenuSceneName = "MainMenu"; // para que no se aplique en el MainMenu

    private static PauseManager instance;

    public bool allowPauseInput = true;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // GameObject persista entre escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // destruye instancias duplicadas
            Destroy(gameObject);
        }

        // asegura que el canvas esté desactivado al inicio
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseCanvas no asignado en PauseManager");
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OptionsPanel no asignado en PauseManager. Si no lo usas, ignora esto.");
        }
    }

    void Update()
    {
        if (!allowPauseInput) return;
        // obtener nombre de escena actual
        string currentScene = SceneManager.GetActiveScene().name;

        // Solo pausa si no estamos en la escena MainMenu
        if (currentScene != mainMenuSceneName)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        else
        {
            if (isPaused)
            {
                isPaused = false;
                Time.timeScale = 1f;
                if (pauseCanvas != null) pauseCanvas.SetActive(false);
                if (optionsPanel != null) optionsPanel.SetActive(false);
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // detiene el tiempo
            if (pauseCanvas != null) pauseCanvas.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // reanudar el tiempo
            if (pauseCanvas != null) pauseCanvas.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }
    }

    public void Resume()
    {
        if (isPaused) // solo reanuda si está pausado
        {
            TogglePause();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SaveGame()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.ManualSaveGame();
            Debug.Log("Game Data Saved! :D");
        }
        else
        {
            Debug.LogError("Error saving data :(");
        }
    }

    // SETTINGS
    public void OpenSettings()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true); // Muestra el panel de opciones
        }
        else
        {
            Debug.LogWarning("El panel de opciones no está asignado o es nulo.");
        }
    }
    
    public void BackToPausePanel()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false); 
        if (pauseCanvas != null) pauseCanvas.SetActive(true); 
    }
}
