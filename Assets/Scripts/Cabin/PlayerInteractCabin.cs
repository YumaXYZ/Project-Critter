using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractCabin : MonoBehaviour
{
    public float interactRange = 2f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                Debug.Log(collider);
            }
        }
    }
}

