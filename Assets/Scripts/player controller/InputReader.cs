using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [Header("Dirección Filtrada")]
    // Usamos variables públicas normales para que cualquier script las lea fácil
    public Vector3 inputDirection;
    public bool hasInputActive;

    [Header("Opción Salomónica (Filtro Anti-Drift)")]
    public bool useSalomonicFilter = false;
    [Range(0f, 0.2f)] public float salomonicThreshold = 0.08f;

    // Se ejecuta automáticamente por el componente PlayerInput (Send Messages)
    private void OnMovement(InputValue value)
    {
        Vector2 inputMovement = value.Get<Vector2>();
        float currentMagnitude = inputMovement.magnitude;

        // Si el filtro está encendido, limpiamos el ruido del mando de PS4
        if (useSalomonicFilter)
        {
            if (currentMagnitude < salomonicThreshold)
            {
                inputMovement = Vector2.zero;
            }
            else
            {
                // Normalizamos para arrancar suave desde el 8%
                float normalizedMagnitude = (currentMagnitude - salomonicThreshold) / (1f - salomonicThreshold);
                inputMovement = inputMovement.normalized * normalizedMagnitude;
            }
        }

        // Pasamos el movimiento de 2D a las 3D de Unity (X, 0, Z)
        inputDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
        hasInputActive = (inputDirection.sqrMagnitude > 0.001f);
    }

    // Se ejecuta al presionar el Bumper Izquierdo (LB)
    private void OnToggleFilter(InputValue value)
    {
        if (value.isPressed)
        {
            useSalomonicFilter = !useSalomonicFilter;
            Debug.Log($"[FILTRO] Cambiado a: {(useSalomonicFilter ? "ACTIVADO" : "DESACTIVADO")}");
        }
    }
}