using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AchievementUIManager : MonoBehaviour
{
  [Header("UI References")]
    public GameObject achievementPanel;           
    public Transform achievementListPanel;     
    public GameObject achievementItemPrefab; 
    public Button achievementsButton;   
    public Button closeButton;                 

    [Header("Achievement Icons")]
    public Sprite defaultAchievementIcon;        
    public Sprite unlockedIcon;                 

    private void Start()
    {
        if (achievementsButton != null)
            achievementsButton.onClick.AddListener(ShowAchievementPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideAchievementPanel);

        if (achievementPanel != null)
            achievementPanel.SetActive(false);
    }

    public void ShowAchievementPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
            PopulateAchievements();
        }
    }

    public void HideAchievementPanel()
    {
        if (achievementPanel != null)
            achievementPanel.SetActive(false);
    }

    private void PopulateAchievements()
    {
        // Clear existing achievement items
        ClearAchievementList();

        // Get all achievements from the manager
        if (AchievementManager.Instance != null)
        {
            List<Achievement> allAchievements = AchievementManager.Instance.GetAllAchievements();

            foreach (Achievement achievement in allAchievements)
            {
                CreateAchievementItem(achievement);
            }
        }
        else
        {
            Debug.LogError("AchievementUIManager: AchievementManager.Instance es nulo. No se pueden cargar los logros.");
        }
    }

    private void ClearAchievementList()
    {
        // Destroy all existing achievement items
        foreach (Transform child in achievementListPanel)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateAchievementItem(Achievement achievement) // Ahora recibe un objeto Achievement
    {
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementListPanel);
        
        AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();
        
        if (itemUI == null)
        {
            Debug.LogWarning($"AchievementUIManager: El prefab '{achievementItemPrefab.name}' no tiene el componente AchievementItemUI. Intentando añadirlo y buscar componentes manualmente.");
            itemUI = itemObj.AddComponent<AchievementItemUI>();
            itemUI.FindUIComponents();
        }

        // Usa el método GetIconSprite() de la clase Achievement
        Sprite mainIcon = achievement.GetIconSprite(); 
        if (mainIcon == null) mainIcon = defaultAchievementIcon; // Fallback si no se encuentra el icono

        // PASA DIRECTAMENTE unlockedIcon. El AchievementItemUI decidirá si lo muestra.
        itemUI.SetupAchievement(achievement, mainIcon, unlockedIcon);
    }
}