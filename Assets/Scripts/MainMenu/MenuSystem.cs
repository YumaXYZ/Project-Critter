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
    
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
    }
}