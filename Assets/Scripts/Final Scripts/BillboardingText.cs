using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardingText : MonoBehaviour
{
    private Transform mainCamera;
    private Transform referenceObject;
    public Canvas canvas;
    private Transform worldSpaceCanvas;
    

    public Vector3 offset;

    private void Start()
    {
        mainCamera = Camera.main.transform;
        referenceObject = transform.parent;
        worldSpaceCanvas = canvas.transform;

        transform.SetParent(worldSpaceCanvas);
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        transform.position = referenceObject.position + offset;
    }
}
