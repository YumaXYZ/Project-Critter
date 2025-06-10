using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class ReadNote : MonoBehaviour
{
    [Header("Texto que se va a enseñar")]
    public GameObject pickUpText;
    
    private bool allowInput; 

    [Header("Audio del item")]
    public AudioClip itemAudio;

    void Start()
    {
        allowInput = true;
        pickUpText.SetActive(false);
    }

    public void Interact()
    {
        if (allowInput)
        {
            AudioSource.PlayClipAtPoint(itemAudio, Camera.main.transform.position, 1.0f);
            allowInput = false;
            pickUpText.SetActive(true);
            Time.timeScale = 0f;
        }

        else
        {
            pickUpText.SetActive(false);
            Time.timeScale = 1f;
            allowInput = true;
        }
        
        
    }
}
