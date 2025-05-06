using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamSwitcher : MonoBehaviour
{
    public Transform Player;
    public CinemachineVirtualCamera activeCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCamera.Priority = 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCamera.Priority = 0;
        }
    }
}
