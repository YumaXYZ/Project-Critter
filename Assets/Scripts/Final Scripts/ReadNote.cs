using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadNote : MonoBehaviour
{
    public GameObject pickUpText;
    public bool allowInput;

    [Header("Sonidos")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    void Start()
    {
        allowInput = true;
        pickUpText.SetActive(false);

        // Intenta obtener el AudioSource del mismo objeto
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si no existe, lo crea automáticamente
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        if (allowInput)
        {
            allowInput = false;
            pickUpText.SetActive(true);
            Time.timeScale = 0f;

            if (openSound != null)
                audioSource.PlayOneShot(openSound);
        }
        else
        {
            pickUpText.SetActive(false);
            Time.timeScale = 1f;
            allowInput = true;

            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }
    }
}
