using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
   [Header("UI Components")]
    public Image achievementIcon;
    public Image statusIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    public void FindUIComponents()
    {
        // Find components in the hierarchy (adjust paths as needed)
        Transform iconContainer = transform.Find("IconContainer");
        if (iconContainer != null)
        {
            achievementIcon = iconContainer.Find("AchievementIcon")?.GetComponent<Image>();
            statusIcon = iconContainer.Find("StatusIcon")?.GetComponent<Image>();
        }

        Transform infoPanel = transform.Find("InfoPanel");
        if (infoPanel != null)
        {
            nameText = infoPanel.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            descriptionText = infoPanel.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        }
    }

    public void SetupAchievement(Achievement achievement, Sprite iconSprite, Sprite statusSprite)
    {
        // Set achievement icon
        if (achievementIcon != null && iconSprite != null)
            achievementIcon.sprite = iconSprite;

        // Set status icon
        if (statusIcon != null && statusSprite != null)
        {
            statusIcon.sprite = statusSprite;
            statusIcon.gameObject.SetActive(true);
        }

        // Set text
        if (nameText != null)
            nameText.text = achievement.title;

        if (descriptionText != null)
            descriptionText.text = achievement.description;

        // Visual feedback for locked achievements
        if (!achievement.isUnlocked)
        {
            // Make locked achievements appear dimmed
            if (achievementIcon != null)
                achievementIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            if (nameText != null)
                nameText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            if (descriptionText != null)
                descriptionText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            // Unlocked achievements appear normal
            if (achievementIcon != null)
                achievementIcon.color = Color.white;
            
            if (nameText != null)
                nameText.color = Color.white;
            
            if (descriptionText != null)
                descriptionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
}
