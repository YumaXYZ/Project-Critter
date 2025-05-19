using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
               if (collider.TryGetComponent(out DoorInteract doorInteract))
               {
               doorInteract.Interact();
               }
            }
        }
    }

    public DoorInteract GetInteractableObject()
    {
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out DoorInteract doorInteract))
            {
                return doorInteract;
            }
        }
        return null;
    }
}
