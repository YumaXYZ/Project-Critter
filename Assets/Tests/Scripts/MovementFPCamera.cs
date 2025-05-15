using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementFPCamera : MonoBehaviour
{
   public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // grados por segundo
    public Transform cameraTransform; // Asigna aquí tu CinemachineVirtualCamera en el Inspector

    private CharacterController characterController;

    // Variables para controlar la rotación del cuerpo basada en la cámara
    public float cameraRotationSpeed = 5f; // Ajusta la velocidad de rotación del cuerpo

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

    }

    private void Update()
    {
        if (cameraTransform != null)
        {
            // Rotación del cuerpo basada en la rotación horizontal de la cámara
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseX = Input.GetAxis("Mouse X");

            // Rotar la cámara (esto ya lo hace Cinemachine POV si lo estás usando)
            // Si no usas Cinemachine POV para la rotación, descomenta estas líneas y ajusta la sensibilidad
            /*
            float cameraRotationX = cameraTransform.localEulerAngles.x - mouseY * cameraRotationSpeed;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);
            cameraTransform.localEulerAngles = new Vector3(cameraRotationX, cameraTransform.localEulerAngles.y, 0f);
            */

            // Rotar el cuerpo horizontalmente basado en el movimiento del ratón en X
            transform.Rotate(Vector3.up * mouseX * cameraRotationSpeed);

            // Movimiento del personaje basado en la orientación actual de la cámara
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(h, 0, v).normalized;

            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * inputDirection.z + camRight * inputDirection.x;

            characterController.SimpleMove(moveDirection * moveSpeed);

        }
    }
}
