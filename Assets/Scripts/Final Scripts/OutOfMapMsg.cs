using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfMapMsg : MonoBehaviour
{
    public TypeWriterGUI typeWriter;
    public string messageLocalizationKey = "outOfMap_text";

    private bool messageShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !messageShown)
        {
            if (LocalizationManager.Instance != null)
            {
                string localizedMessage = LocalizationManager.Instance.GetLocalizedValue(messageLocalizationKey);
                typeWriter.ShowMessage(localizedMessage);
            }
            else
            {
                Debug.LogWarning("OutOfMapMsg: LocalizationManager not found! Showing raw key: " + messageLocalizationKey);
                typeWriter.ShowMessage(messageLocalizationKey); 
            }

            messageShown = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            messageShown = false;
        }
    }
    
}
