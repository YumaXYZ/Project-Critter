using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjInteractable : MonoBehaviour
{
    public string interactionMsg = "¿Qué es esto?";

    public string GetMessage()
    {
        return interactionMsg;
    }
}
