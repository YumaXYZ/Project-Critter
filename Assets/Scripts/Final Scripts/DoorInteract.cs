using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    public void Interact()
    {
        if(SceneManager.GetActiveScene().name == "Cabin 1")
        {
            SceneManager.LoadScene("Cemetery");
        }
        else
        {
        SceneManager.LoadScene("Cabin 1");
        }
    }
}
