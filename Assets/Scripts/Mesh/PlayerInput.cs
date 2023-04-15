using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float deformationForce = 10;
    public float deformationRadius = 0.6f;
    public InputActionReference mouseLeft;
    public Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        mouseLeft.action.Enable();
        mouseLeft.action.performed += MouseLeft;
        mouseLeft.action.canceled += MouseLeft;
    }

    bool mouseDown = false;
    private void MouseLeft(InputAction.CallbackContext obj)
    {
        mouseDown = !mouseDown;
    }

    public Rigidbody touch;
    // Update is called once per frame
    void Update()
    {
        if(mouseDown )
        {
            Ray inputRay = new Ray(cam.position, cam.forward);
            if (Physics.Raycast(inputRay, out var hit))
            {
                var deformer = hit.collider.GetComponent<MeshDeformer>();
                if (deformer != null)
                    deformer.ApplyForce(
                        hit.point + hit.normal * deformationRadius, 
                        deformationForce,
                        Time.deltaTime);
                touch.position = hit.point;
                touch.transform.localScale = Vector3.one * deformationRadius;
            }
        }
    }
}
