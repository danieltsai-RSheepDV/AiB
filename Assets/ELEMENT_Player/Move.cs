using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Move : MonoBehaviour
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
    [SerializeField] private Collider standing;
    [SerializeField] private Collider swimming;
    public void ToggleSwim(bool b)
    {
        isSwimming = b;
        rb.useGravity = !b;
        standing.enabled = !b;
        swimming.enabled = b;
        breathingPaused = !b;
    }

    private readonly Vector3 mapCenter = new Vector3(150, 0, 150);
    public void LimitDistance()
    {
        Vector3 XZPosition = transform.position;
        XZPosition.y = 0;
        XZPosition -= mapCenter;
        if (XZPosition.magnitude > 200)
        {
            XZPosition = XZPosition.normalized * 200 + mapCenter;
            transform.position = XZPosition + Vector3.up * transform.position.y;
        }
    }

    [SerializeField] private MicrophoneInput mic;
    [SerializeField] private Volume breathVolume;
    [SerializeField] private Volume chokingVolume;
    private bool breathingPaused = false;
    private bool isInhaling = true;
    private float threshold = 0.2f;
    private float airMeter = 0;
    private float maxAir = 10f;
    
    private void BreathingTracker()
    {
        if (breathingPaused)
        {
            breathVolume.weight = 0;
            chokingVolume.weight = 0;
            airMeter = maxAir;
            isInhaling = true;
            return;
        }
        if (isInhaling && mic.breathValue > 1 - threshold)
        {
            airMeter = maxAir;
            isInhaling = false;
            return;
        }

        if(!isInhaling && mic.breathValue < threshold)
        {
            airMeter = maxAir;
            isInhaling = true;
            return;
        }

        airMeter -= Time.deltaTime;
        chokingVolume.weight = Math.Max(0, 4f - airMeter)/4f;
        breathVolume.weight = mic.breathValue;
        if (airMeter < 0)
        {
            ToggleSwim(false);
        }
    }

    private void Update()
    {
        LimitDistance();
        // BreathingTracker();
        
        // Handle camera rotation
        Vector2 lookVector = lookAction.ReadValue<Vector2>();
        yRotation += lookVector.x * lookSpeed;
        xRotation -= lookVector.y * lookSpeed;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        RenderSettings.fog = cameraTransform.position.y < 0f;
        if (cameraTransform.position.y < 0f && !ps.isPlaying)
        {
            ps.Play();
        }
        else if(cameraTransform.position.y > 0f && !ps.isStopped)
        {
            ps.Stop();
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

    private void FixedUpdate()
    {
        Vector2 moveVector = moveAction.ReadValue<Vector2>();
        if (isSwimming)
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