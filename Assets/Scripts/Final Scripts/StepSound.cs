using UnityEngine;
public class FootstepLoop : MonoBehaviour
{
    public CharacterController controller;
    public AudioSource audioSource;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool isWalking = controller.isGrounded && controller.velocity.magnitude > 0.2f;

        if (isWalking && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!isWalking && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
