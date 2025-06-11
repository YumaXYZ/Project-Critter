using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeWriterGUI : MonoBehaviour
{
    public TextMeshProUGUI TextComponent;
    private float timePerChar = 0.05f;
    public float timeAfterFinished = 2f;

    private string currentMessage = null;
    private float timer = 0;
    private int charIndex = 0;
    private float finishedDisplayTimer = 0f;
    private bool messageFinished = false;

    public GameObject pause;
    public GameObject player;
    public GameObject interactuableTextCanvas;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!messageFinished && currentMessage != null)
            {
                TextComponent.text = currentMessage;
                charIndex = currentMessage.Length;
                currentMessage = null;
                messageFinished = true;
                finishedDisplayTimer = 10f;
            }
            else if (messageFinished)
            {
                ClearMessage();
            }
        }

        if (string.IsNullOrEmpty(currentMessage))
        {
            if (messageFinished)
            {
                finishedDisplayTimer -= Time.unscaledDeltaTime;
                if (finishedDisplayTimer <= 0)
                {
                    Time.timeScale = 1f;
                    pause.GetComponent<PauseManager>().allowPauseInput = true;
                    player.GetComponent<PlayerInteract>().allowInteraction = true;
                    interactuableTextCanvas.SetActive(true);
                    gameObject.SetActive(false);
                    messageFinished = false;
                }
            }
            return;
        }

        timer -= Time.unscaledDeltaTime;
        if (timer <= 0)
        {
            timer += timePerChar;
            charIndex++;

            string tmpMessage = currentMessage.Substring(0, charIndex);
            TextComponent.text = tmpMessage;

            if (charIndex >= currentMessage.Length)
            {
                currentMessage = null;
                messageFinished = true;
                finishedDisplayTimer = timeAfterFinished;
            }
        }
    }

    public void ShowMessage(string messageToShow)
    {
        Time.timeScale = 0f;
        pause.GetComponent<PauseManager>().allowPauseInput = false;
        player.GetComponent<PlayerInteract>().allowInteraction = false;
        interactuableTextCanvas.SetActive(false);

        currentMessage = messageToShow;
        charIndex = 0;
        TextComponent.text = "";
        messageFinished = false;
        finishedDisplayTimer = 0f;
        timer = 0f;

        gameObject.SetActive(true);
    }

    public void ClearMessage()
    {
        Time.timeScale = 1f;
        pause.GetComponent<PauseManager>().allowPauseInput = true;
        player.GetComponent<PlayerInteract>().allowInteraction = true;
        interactuableTextCanvas.SetActive(true);

        currentMessage = null;
        TextComponent.text = "";
        messageFinished = false;
        finishedDisplayTimer = 0f;

        gameObject.SetActive(false);
    }
}
