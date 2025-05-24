using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public TypeWriterGUI textBoxGUI;
    [SerializeField] private float interactRange = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {   
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if(collider.TryGetComponent(out ObjInteractable objInteractable))
                {
                    string objMsg = objInteractable.GetMessage();
                    if (textBoxGUI != null && !string.IsNullOrEmpty(objMsg)) textBoxGUI.ShowMessage(objMsg);
                }
                if (collider.TryGetComponent(out DoorInteract doorInteract))
                {
                    doorInteract.Interact();
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