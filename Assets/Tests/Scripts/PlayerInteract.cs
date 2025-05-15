using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< HEAD

public class PlayerInteract : MonoBehaviour
{
=======
using UnityEngine.UIElements;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
>>>>>>> interaction-system
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
<<<<<<< HEAD
            float interactRange = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                Debug.Log(collider);
            }
        }
    }
=======
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
>>>>>>> interaction-system
}
