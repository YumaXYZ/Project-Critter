using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AchievementUIManager : MonoBehaviour
{
  [Header("UI References")]
    public GameObject achievementPanel;           
    public Transform achievementListPanel;        // el panel donde saldrán los logros (la cajita)
    public GameObject achievementItemPrefab;      // AchievementItem prefab
    public Button achievementsButton;            // el botón que abre el panel de los logros en el main menu
    public Button closeButton;                   // el closeButton del panel de logros

    [Header("Achievement Icons")]
    public Sprite defaultAchievementIcon;        // un icono default
    public Sprite lockedIcon;                    // icono de candadito o algo
    public Sprite unlockedIcon;                  // icono de logro desbloqueado

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

            // Create UI item for each achievement
            foreach (Achievement achievement in allAchievements)
            {
                CreateAchievementItem(achievement);
            }
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

    private void CreateAchievementItem(Achievement achievement)
    {
        // Instantiate the achievement item
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementListPanel);
        
        // Get the UI components (adjust these names to match your prefab structure)
        AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();
        
        if (itemUI == null)
        {
            // If no component, find manually
            itemUI = itemObj.AddComponent<AchievementItemUI>();
            itemUI.FindUIComponents();
        }

        // Populate the UI with achievement data
        itemUI.SetupAchievement(achievement, defaultAchievementIcon, 
                               achievement.isUnlocked ? unlockedIcon : lockedIcon);
    }
}