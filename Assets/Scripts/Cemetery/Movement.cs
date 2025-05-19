using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float characterSpeed; // Variable publica para controlar la velocidad
    public float rotationSpeed; // Variable publica para controlar la velocidad

    private CharacterController characterController;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Almacenamos el input "horizontal" por defecto de Unity en una variables (A,D, flechas ...)
        float verticalInput = Input.GetAxis("Vertical");  // Almacenamos el input "vertical" por defecto de Unity en una variables (W,S, flechas ...)

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput); // Creamos un vector direccion para controlar la direccion del personaje
        float magnitude = Mathf.Clamp01(movementDirection.magnitude) * characterSpeed;
       // movementDirection.Normalize(); // Normalizamos la direccion para que en caso de moverse en diagonal no se multiplique la velocidad mas de lo deseado

        characterController.SimpleMove(movementDirection * magnitude);

        // Chequeamos la direccion del personaje
        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("isMoving", true);
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}