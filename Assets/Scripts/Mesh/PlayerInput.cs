using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float deformationForce = 10;
    public float deformationOffset = 0.1f;
    public InputActionReference mouseLeft, mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        mouseLeft.action.Enable();
        mouseLeft.action.performed += MouseLeft;
        mouseLeft.action.canceled += MouseLeft;

        mousePosition.action.Enable();
        mousePosition.action.performed += MousePosition;
    }

    public Vector2 screenPos = new Vector2();
    private void MousePosition(InputAction.CallbackContext obj)
    {
        screenPos = obj.ReadValue<Vector2>();
    }

    bool mouseDown = false;
    private void MouseLeft(InputAction.CallbackContext obj)
    {
        mouseDown = !mouseDown;
    }

    // Update is called once per frame
    void Update()
    {
        if(mouseDown )
        {
            Ray inputRay = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(inputRay, out var hit))
            {
                var deformer = hit.collider.GetComponent<MeshDeformer>();
                if (deformer != null)
                    deformer.ApplyForce(
                        hit.point + hit.normal * deformationOffset, 
                        deformationForce,
                        Time.deltaTime);
            }
        }
    }
}
