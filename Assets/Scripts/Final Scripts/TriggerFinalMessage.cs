using UnityEngine;

public class TriggerFinalMessage : MonoBehaviour
{
    public GameObject finalMessageText;
    public GameObject finalMessageLight;

    private bool messageDisplayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !messageDisplayed)
        {
            finalMessageText.SetActive(true);
            finalMessageLight.SetActive(true);
            messageDisplayed = true;
        }
    }
}