using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    public LayerMask groundMask;
    
    public float moveSpeed = 5f;
    public float swimSpeed = 2f;
    public float lookSpeed = 2f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    private AiBInput inputActionSet;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private float yRotation;
    private float xRotation;

    private Rigidbody rb;

    private bool isSwimming = false;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActionSet = new AiBInput();
        moveAction = inputActionSet.Player.Move;
        lookAction = inputActionSet.Player.Look;
        jumpAction = inputActionSet.Player.Jump;
    }

    private void OnEnable()
    {
        // Enable the PlayerInput actions
        inputActionSet.Enable();
    }

    private void OnDisable()
    {
        // Disable the PlayerInput actions
        inputActionSet.Disable();
    }

    private void Update()
    {
        if (isSwimming)
        {
            Vector2 moveVector = moveAction.ReadValue<Vector2>();
            if (rb.velocity.magnitude < swimSpeed)
            {
                rb.AddForce(cameraTransform.right * moveVector.x + cameraTransform.forward * moveVector.y);
            }

            if (transform.position.y > 0.25f)
            {
                isSwimming = false;
                rb.useGravity = true;
            }
        }
        else
        {
            Vector2 moveVector = moveAction.ReadValue<Vector2>();
            if (transform.position.y < 0.25f)
            {
                rb.AddForce(Vector3.up * (Mathf.Max(0, 5f * Mathf.Abs(transform.position.y) - 1f * rb.velocity.y)));
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 1
                        , groundMask
                    ))
                {
                
                }
                else
                {if (rb.velocity.magnitude < swimSpeed)
                    {
                        rb.AddForce(transform.right * moveVector.x + transform.forward * moveVector.y);
                    }
                }
            }
            else
            {
                rb.velocity = (transform.right * moveVector.x + transform.forward * moveVector.y) * moveSpeed + Vector3.up * rb.velocity.y;
            }

            if (jumpAction.WasPressedThisFrame())
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                isSwimming = true;
                rb.useGravity = false;
            }
        }
        
        // Handle camera rotation
        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        yRotation += lookVector.x * lookSpeed;
        xRotation -= lookVector.y * lookSpeed;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}