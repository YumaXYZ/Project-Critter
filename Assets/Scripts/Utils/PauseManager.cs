using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas; // canvas de pausa, arrastrarlo al inspector

    private bool isPaused = false;

    public string mainMenuSceneName = "MainMenu"; // para que no se aplique en el MainMenu

    private static PauseManager instance;

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
    }

    void Update()
    {
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
                if (pauseCanvas != null)
                {
                    pauseCanvas.SetActive(false);
                }
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // detiene el tiempo
            if (pauseCanvas != null)
            {
                pauseCanvas.SetActive(true);
            }
        }
        else
        {
            Time.timeScale = 1f; // reanudar el tiempo
            if (pauseCanvas != null)
            {
                pauseCanvas.SetActive(false);
            }
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

    public void Quit()
    {
        Debug.Log("SALIENDO");
        Application.Quit();
    }
}
