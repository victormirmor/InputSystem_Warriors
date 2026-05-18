using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float smoothingSpeed = 1f;

    private Rigidbody rb;
    private Camera mainCamera;
    
    private Vector3 currentDirection;
    private Vector3 rawDirection;
    private Vector3 smoothDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Se desacopla de GameManager en Start para buscar la cámara de forma segura
        mainCamera = GameManager.Instance?.mainCamera ?? Camera.main;
    }

    public void ProcessMovement(Vector3 inputDirection, bool hasInput)
    {
        CalculateDesiredDirection(inputDirection);
        ConvertDirectionFromRawToSmooth();
        Move(smoothDirection);
        Turn(smoothDirection, hasInput);
    }

    private void CalculateDesiredDirection(Vector3 inputDirection)
    {
        // Lógica de proyección de cámara del repositorio original
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        rawDirection = (camRight * inputDirection.x) + (camForward * inputDirection.z);
    }

    private void ConvertDirectionFromRawToSmooth()
    {
        smoothDirection = Vector3.MoveTowards(smoothDirection, rawDirection, smoothingSpeed * Time.fixedDeltaTime);
    }

    private void Move(Vector3 direction)
    {
        Vector3 movement = direction * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + movement);
    }

    private void Turn(Vector3 direction, bool hasInput)
    {
        if (hasInput && direction.sqrMagnitude > 0.001f)
        {
            Quaternion newRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(newRotation);
        }
    }

    public float GetCurrentSpeedMagnitude() => smoothDirection.sqrMagnitude;
}