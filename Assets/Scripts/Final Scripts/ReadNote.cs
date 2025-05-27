using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class ReadNote : MonoBehaviour
{
    public GameObject pickUpText;
    public bool allowInput; 

    void Start()
    {
        allowInput = true;
        pickUpText.SetActive(false);
    }

    public void Interact()
    {
        if (allowInput)
        {
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
