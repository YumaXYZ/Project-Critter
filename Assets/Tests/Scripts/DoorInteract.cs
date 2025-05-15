using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour
{
    public void Interact()
    {
        SceneManager.LoadScene("Test_Interactions2");
    }
}
