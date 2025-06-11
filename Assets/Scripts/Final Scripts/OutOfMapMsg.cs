using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfMapMsg : MonoBehaviour
{
    public TypeWriterGUI typeWriter;
    public string mensaje;

    private bool mensajeMostrado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !mensajeMostrado)
        {
            typeWriter.ShowMessage(mensaje);
            mensajeMostrado = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mensajeMostrado = false; 
        }
    }
    
}
