using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionCabin : MonoBehaviour
{
    public float interactionRange = 2f;
    public string targetTag = "interactableObj";
    public TypeWriterUI textBoxUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactionRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.CompareTag(targetTag))
                {
                    //if (collider.TryGetComponent(out ItemInteractable itemInteractable))
                    //{
                    //    string itemMsg = itemInteractable.GetMessage();
                    //    if (textBoxUI != null && !string.IsNullOrEmpty(itemMsg)) textBoxUI.ShowMessage(itemMsg);
                    //    // itemInteractable.Interact();
                    //}
                    //// Debug.Log("Interacted with: " + collider.gameObject.name);   
                }
            }
        }
    }
}
