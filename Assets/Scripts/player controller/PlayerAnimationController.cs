using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private int movementHash;
    private int attackHash;

    void Awake()
    {
        movementHash = Animator.StringToHash("Movement");
        attackHash = Animator.StringToHash("Attack");
    }

    public void UpdateMovementAnimation(float speedMagnitude)
    {
        animator.SetFloat(movementHash, speedMagnitude);
    }

    public void TriggerAttackAnimation()
    {
        animator.SetTrigger(attackHash);
    }
}