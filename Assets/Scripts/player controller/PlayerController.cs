using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Vínculo con el Lector de Mando")]
    // Arrastrás el objeto con el script InputReader acá en el Inspector
    public InputReader mando; 

    // VARIABLES ORIGINALES DEL REPOSITORIO (Mantenidas intactas)
    private Camera mainCamera;

    [Header("Physics")]
    public Rigidbody playerRigidbody;

    [Header("Animation")]
    public Animator playerAnimator;
    private int playerMovementID;
    private int playerAttackID;

    [Header("Input Base")]
    public PlayerInput playerInput;
    private string actionMapGameplay = "Player Controls";
    private string actionMapMenu = "Menu Controls";

    [Header("Movement Settings")]
    public float movementSpeed = 3;
    public float smoothingSpeed = 1;
    private Vector3 rawDirection;
    private Vector3 smoothDirection;
    private Vector3 movement;

    void Start()
    {
        mainCamera = GameManager.Instance.mainCamera;
        playerMovementID = Animator.StringToHash("Movement");
        playerAttackID = Animator.StringToHash("Attack");
    }

    void FixedUpdate()
    {
        CalculateDesiredDirection();
        ConvertDirectionFromRawToSmooth();
        MoveThePlayer();
        TurnThePlayer();
        AnimatePlayerMovement();
    }

    void CalculateDesiredDirection()
    {
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // REEMPLAZO SIMPLE: En vez de usar variables internas, leemos las variables del InputReader
        rawDirection = (camRight * mando.inputDirection.x) + (camForward * mando.inputDirection.z);
    }

    void ConvertDirectionFromRawToSmooth()
    {
        smoothDirection = Vector3.MoveTowards(smoothDirection, rawDirection, smoothingSpeed * Time.fixedDeltaTime);
    }

    void MoveThePlayer()
    {
        movement = smoothDirection * movementSpeed * Time.deltaTime;
        playerRigidbody.MovePosition(transform.position + movement);
    }

    void TurnThePlayer()
    {
        // REEMPLAZO SIMPLE: Leemos si el stick se está moviendo desde el InputReader
        if (mando.hasInputActive == true)
        {
            Quaternion newRotation = Quaternion.LookRotation(smoothDirection);
            playerRigidbody.MoveRotation(newRotation);
        }
    }

    void AnimatePlayerMovement()
    {
        playerAnimator.SetFloat(playerMovementID, smoothDirection.sqrMagnitude);
    }

    // Acción original para el ataque
    private void OnAttack(InputValue value)
    {
        playerAnimator.SetTrigger("Attack");
    }

    // Control de menús original
    /*private void OnOpenPauseMenu(InputValue value)
    {
        if(value.isPressed) GameManager.Instance.TogglePauseMenu(true);
    }*/

    /*private void OnClosePauseMenu(InputValue value)
    {
        if(value.isPressed) GameManager.Instance.TogglePauseMenu(false);
    }*/

    public void EnableGameplayControls() => playerInput.SwitchCurrentActionMap(actionMapGameplay);
    public void EnablePauseMenuControls() => playerInput.SwitchCurrentActionMap(actionMapMenu);
    public PlayerInput GetPlayerInput() => playerInput;
}