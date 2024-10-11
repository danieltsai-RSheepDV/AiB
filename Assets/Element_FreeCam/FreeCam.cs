using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FreeCam : MonoBehaviour
{
    public LayerMask groundMask;
    
    public float moveSpeed = 5f;
    public float swimSpeed = 2f;
    public float lookSpeed = 2f;
    public Transform cameraTransform;

    public float waterLevel = 0.25f;
    
    private AiBInput inputActionSet;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction flyAction;
    private InputAction descendAction;

    private float yRotation;
    private float xRotation;

    private Rigidbody rb;

    private ParticleSystem ps;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActionSet = new AiBInput();
        moveAction = inputActionSet.Player.Move;
        lookAction = inputActionSet.Player.Look;
        jumpAction = inputActionSet.Player.Jump;
        flyAction = inputActionSet.Player.Fly;
        descendAction = inputActionSet.Player.Descend;
        Cursor.lockState = CursorLockMode.Locked;

        ps = GetComponent<ParticleSystem>();
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
    
    private bool isSwimming = false;
    private bool isFlying = false;
    [SerializeField] private Collider standing;
    [SerializeField] private Collider swimming;
    public void ToggleSwim(bool b)
    {
        isSwimming = b;
        rb.useGravity = !b;
        standing.enabled = !b;
        swimming.enabled = b;
        rb.drag = b ? 1f : 0f;
    }

    private void Update()
    {
        // Handle camera rotation
        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        yRotation += lookVector.x * lookSpeed;
        xRotation -= lookVector.y * lookSpeed;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yRotation, 0f), Time.deltaTime);
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, Quaternion.Euler(xRotation, 0f, 0f), Time.deltaTime);
        
        RenderSettings.fog = cameraTransform.position.y < 0f;
        if (cameraTransform.position.y < 0f && !ps.isPlaying)
        {
            ps.Play();
        }
        else if(cameraTransform.position.y > 0f && !ps.isStopped)
        {
            ps.Stop();
        }

        if (!isFlying)
        {
            if (flyAction.WasPressedThisFrame())
            {
                isFlying = !isFlying;
                rb.useGravity = false;
            }
            
            if (isSwimming)
            {
                if (transform.position.y > 0.1f)
                {
                    ToggleSwim(false);
                }
            }
            else if (transform.position.y < 0 && jumpAction.WasPressedThisFrame())
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                ToggleSwim(true);
            }
        }
        else
        {
            if (flyAction.WasPressedThisFrame())
            {
                isFlying = !isFlying;
                rb.useGravity = true;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 moveVector = moveAction.ReadValue<Vector2>();
        if (isFlying)
        {
            Vector3 dir = (cameraTransform.right * moveVector.x + cameraTransform.forward * moveVector.y);
            dir.y = 0;
            rb.velocity = dir.normalized * 5f;
            if (jumpAction.IsPressed()) rb.velocity += Vector3.up * 5f;
            if (descendAction.IsPressed()) rb.velocity -= Vector3.up * 5f;
        }
        else if (isSwimming)
        {
            if (rb.velocity.magnitude < swimSpeed)
            {
                rb.AddForce((cameraTransform.right * moveVector.x + cameraTransform.forward * moveVector.y) * 3f);
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _, 2f, groundMask))
            {
                rb.velocity = (transform.right * moveVector.x + transform.forward * moveVector.y) * moveSpeed + Vector3.up * rb.velocity.y;
            }
            else
            {
                rb.AddForce((cameraTransform.right * moveVector.x + cameraTransform.forward * moveVector.y) * 3f);
            }
            
            if (transform.position.y < 0)
            {
                rb.AddForce(Vector3.up * Mathf.Max(0, 40f * Mathf.Abs(transform.position.y) - 10f * rb.velocity.y));
            }
        }
    }
}