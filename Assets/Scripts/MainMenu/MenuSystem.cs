using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    [Header("Achievement UI")]
    public GameObject achievementPanel;           
    public GameObject optionsPanel;
    public string FirstGameSceneName = "Cemetery";

    public void Play()
    {
          if (GameState.Instance != null)
        {
            // GameState se encarga de todo el flujo de carga,
            // incluyendo la carga de la escena correcta.
            GameState.Instance.LoadGameFromSave();
            Debug.Log("Solicitando cargar partida guardada...");
        }
        else
        {
            Debug.LogError("MenuSystem: GameState no encontrado. Cargando la primera escena de juego por defecto.");
            SceneManager.LoadScene(FirstGameSceneName); // Fallback
        }
    }

    public void Achievements()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }
    }
    public void Options()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void HideOptionsPanel()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("SALIENDO");
        Application.Quit();

        // para simularlo en UNITY
        UnityEditor.EditorApplication.isPlaying = false;
    }
}