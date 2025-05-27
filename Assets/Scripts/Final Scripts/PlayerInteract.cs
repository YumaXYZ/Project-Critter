using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public TypeWriterGUI textBoxGUI;
    [SerializeField] private float interactRange = 2f;
    
    public bool allowInteraction = true;

    private void Update()
    {
        if (!allowInteraction) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out ObjInteractable objInteractable))
                {
                    string objMsg = objInteractable.GetMessage();
                    if (textBoxGUI != null && !string.IsNullOrEmpty(objMsg)) textBoxGUI.ShowMessage(objMsg);
                }
                if (collider.TryGetComponent(out DoorInteract doorInteract))
                {
                    doorInteract.Interact();
                }
                if (collider.TryGetComponent(out KeyPickup keyPickup))
                {
                    keyPickup.Interact();
                }
                if (collider.TryGetComponent(out ReadNote readNote))
                {
                    readNote.Interact();
                }
            }
        }
    }

    public ObjInteractable GetInteractableObj()
    {
        List<ObjInteractable> objInteractablesList = new List<ObjInteractable> ();
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out ObjInteractable objInteractable))
            {
                objInteractablesList.Add(objInteractable);
            }
        }

        ObjInteractable closetObjInteractable = null;
        foreach (ObjInteractable objInteractable in objInteractablesList)
        {
            if(closetObjInteractable == null)
            {
                closetObjInteractable = objInteractable;
            }
            else
            {
                if(Vector3.Distance(transform.position, objInteractable.transform.position) < 
                    Vector3.Distance(transform.position, closetObjInteractable.transform.position))
                {
                    closetObjInteractable=objInteractable;
                }
            }
        }
        return closetObjInteractable;
    }
}