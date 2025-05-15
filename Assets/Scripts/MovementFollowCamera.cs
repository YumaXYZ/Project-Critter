using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementFollowCamera : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // grados por segundo

    private CharacterController characterController;
    private Animator animator;

    public Transform cameraTransform; // La cámara fija activa

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(h, 0, v);
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        // Convertir input a dirección relativa a la cámara
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * inputDirection.z + camRight * inputDirection.x;

        // Mover al personaje
        characterController.SimpleMove(moveDirection * moveSpeed);

        // Rotar hacia la dirección de movimiento si hay input
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

    }

    public void ChangeCamera(Transform newCamera)
    {
        cameraTransform = newCamera;
    }

}

