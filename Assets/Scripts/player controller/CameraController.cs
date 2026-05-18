using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [Tooltip("Arrastrá acá al personaje del jugador")]
    public Transform target;

    [Header("Configuración de Posición (Estilo MOBA)")]
    [Tooltip("Distancia horizontal y vertical relativa al jugador")]
    public Vector3 cameraOffset = new Vector3(0f, 15f, -10f);

    [Header("Suavizado")]
    [Tooltip("Qué tan rápido la cámara alcanza al jugador. Valores más altos = más instantáneo")]
    public float smoothSpeed = 5f;

    void Start()
    {
        // Si te olvidás de arrastrar el objetivo, el script intenta buscarlo por Tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("[CÁMARA] No se encontró ningún objetivo con el Tag 'Player'. Asignalo en el Inspector.");
            }
        }

        // Configuración inicial de rotación para mirar hacia el mapa
        ApplyInitialRotation();
    }

    void LateUpdate()
    {
        // Se usa LateUpdate para garantizar que el personaje ya se movió en el FixedUpdate 
        // antes de que la cámara intente calcular su nueva posición.
        if (target == null) return;

        MoveCamera();
    }

    private void MoveCamera()
    {
        // Calcula la posición ideal en base a dónde está el jugador en el mundo
        Vector3 desiredPosition = target.position + cameraOffset;
        
        // Interpola linealmente para que la cámara no se mueva a tirones
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Aplica la posición calculada
        transform.position = smoothedPosition;
    }

    private void ApplyInitialRotation()
    {
        // Fuerza a la cámara a mirar en diagonal hacia abajo apuntando al offset inverso
        if (cameraOffset != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-cameraOffset.normalized);
        }
    }
}