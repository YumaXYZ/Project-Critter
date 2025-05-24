using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TypeWriterGUI : MonoBehaviour
{
    public Text TextComponent;
    private float timePerChar = 0.05f;
    public float timeAfterFinished = 2f; // tiempo en segundos que dura el mensaje después de acabar de escribirse

    private string currentMessage = null;
    private float timer = 0;
    private int charIndex = 0;
    private float finishedDisplayTimer = 0f;
    private bool messageFinished = false;

    private void Update()
    {
        if (string.IsNullOrEmpty(currentMessage))
        {
            if (messageFinished)
            {
                finishedDisplayTimer -= Time.deltaTime;
                if (finishedDisplayTimer <= 0)
                {
                    gameObject.SetActive(false);
                    messageFinished = false;
                }
            }
            return;
        }

        timer -= Time.deltaTime;
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
        currentMessage = messageToShow;
        charIndex = 0;
        TextComponent.text = "";
        messageFinished = false;
        finishedDisplayTimer = 0f;

        gameObject.SetActive(true);
    }

    
    public void ClearMessage()
    {
        currentMessage = null;
        TextComponent.text = "";
        messageFinished = false;
        finishedDisplayTimer = 0f;

        gameObject.SetActive(false);
    }
}
