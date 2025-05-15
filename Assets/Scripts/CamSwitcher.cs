using UnityEngine;
using Cinemachine;

public class CamSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera activeCamara;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCamara.Priority = 1;
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCamara.Priority = 0;
        }
    }
}
