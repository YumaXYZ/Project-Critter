using UnityEngine;

public class PlayerSteps : MonoBehaviour
{
    public CharacterController characterController;
    public AudioSource audioSource;

    void Update()
    {

        Vector3 moving = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        
        if (moving.magnitude > 0.1f)
        {
           
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
           
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}

