using UnityEngine;
using UnityEngine.InputSystem;

public class MobaCameraController : MonoBehaviour
{
    [Header("Objetivo (Héroe)")]
    public Transform target;

    [Header("Configuración de Distancia (Offset)")]
    public Vector3 defaultOffset = new Vector3(0f, 15f, -10f);

    [Header("Velocidades")]
    [Tooltip("Velocidad con la que se desplaza libremente la cámara con el stick derecho")]
    public float panSpeed = 15f;
    [Tooltip("Qué tan rápido la cámara vuelve o sigue al jugador de forma suave")]
    public float smoothSpeed = 5f;

    [Header("Límites del Mapa (Filtro de Seguridad)")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    // Variables internas para el estado de la cámara
    private Vector3 currentPosition;
    private Vector2 cameraInput;
    private bool isCameraFree = false;

    void Start()
    {
        if (target != null)
        {
            // Arrancamos posicionados encima del jugador
            currentPosition = target.position + defaultOffset;
            transform.position = currentPosition;
        }
        
        ApplyInitialRotation();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Si el stick derecho se mueve, pasamos a Modo Libre
        if (cameraInput.sqrMagnitude > 0.01f)
        {
            isCameraFree = true;
            MoveFreeCamera();
        }
        else
        {
            // Si no tocamos el stick derecho, la cámara sigue suavemente al héroe (Estilo F1 en Dota)
            if (!isCameraFree)
            {
                FollowTarget();
            }
        }
    }

    private void MoveFreeCamera()
    {
        // El stick derecho mueve la cámara de forma absoluta en el plano XZ del mundo
        Vector3 move = new Vector3(cameraInput.x, 0f, cameraInput.y) * panSpeed * Time.deltaTime;
        currentPosition += move;

        // Aplicamos los límites para que el usuario no se vaya al vacío del motor
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        currentPosition.z = Mathf.Clamp(currentPosition.z, minZ, maxZ);

        transform.position = Vector3.Lerp(transform.position, currentPosition, smoothSpeed * Time.deltaTime);
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + defaultOffset;
        currentPosition = desiredPosition; // Sincronizamos la posición interna
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    private void ApplyInitialRotation()
    {
        if (defaultOffset != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-defaultOffset.normalized);
        }
    }

    // --- CALLBACKS DEL NEW INPUT SYSTEM (Stick Derecho) ---
    // Asegurate de tener una acción llamada "CameraLook" mapeada al Right Stick (<Gamepad>/rightStick)
    private void OnCameraLook(InputValue value)
    {
        cameraInput = value.Get<Vector2>();
    }

    // --- BOTÓN PARA VOLVER A CENTRAR LA CÁMARA (Equivalente a F1 / Barra en tu tabla) ---
    // Mapeado a presionar el Stick Izquierdo (LSB / L3 - <Gamepad>/leftStickPress)
    private void OnSelectHero(InputValue value)
    {
        if (value.isPressed)
        {
            isCameraFree = false; // Desactivamos el modo libre para que encaje de nuevo con el héroe
            Debug.Log("[CÁMARA] Cámara reenfoada en el héroe (F1 Emulado).");
        }
    }
}