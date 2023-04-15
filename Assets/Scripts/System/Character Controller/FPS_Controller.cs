using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPS_Controller : MonoBehaviour
{
    [Range(0.0001f, 5f)]
    public float sensitivity = 0.1f;
    public RotationController rotationController;
    public CharacterController characterController;
    public InputActionReference inputRotation, inputMovement, inputJump;

    private void Awake()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        inputRotation.action.Enable();
        inputRotation.action.performed += PerformedInputRotation;

        inputMovement.action.Enable();
        inputMovement.action.performed += PerformedInputMovement;
        inputMovement.action.canceled += CanceledInputMovement;

        inputJump.action.Enable();
        inputJump.action.performed += PerformedInputJump;
    }

    private void PerformedInputJump(InputAction.CallbackContext obj)
    {
        if (characterController)
            characterController.TriggerJump();
        else
            Debug.LogError("You need to assign a CharacterController Component!", gameObject);
    }

    Vector2 _inputMovement = Vector2.zero;
    private void PerformedInputMovement(InputAction.CallbackContext obj)
    {   
        _inputMovement = obj.ReadValue<Vector2>();
    }
    private void CanceledInputMovement(InputAction.CallbackContext obj)
    {
        _inputMovement = Vector2.zero;
    }

    Vector2 _inputRotation = Vector2.zero;
    private void PerformedInputRotation(InputAction.CallbackContext obj)
    {
        _inputRotation = obj.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (rotationController)
            rotationController.EulerAngles += new Vector3(-_inputRotation.y, _inputRotation.x) * sensitivity;
        else
            Debug.LogError("You need to assign a RotationController Component!", gameObject);
        _inputRotation *= 0;

        if (characterController)
            characterController.inputMovement = _inputMovement;
        else
            Debug.LogError("You need to assign a CharacterController Component!", gameObject);
    }
}
