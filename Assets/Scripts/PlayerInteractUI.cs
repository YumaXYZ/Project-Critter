using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerObject;
    [SerializeField] private PlayerInteract playerInteract;

    private void Update()
    {
        if(playerInteract.GetInteractableObject() != null)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        containerObject.SetActive(true);
    }

    private void Hide()
    {
        containerObject.SetActive(false);
    }
}
