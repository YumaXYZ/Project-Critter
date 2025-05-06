using UnityEngine;
using Cinemachine;

public class CamSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera activeCam;
    public Transform Player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCam.Priority = 1;
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCam.Priority = 1;
        }
    }
}
