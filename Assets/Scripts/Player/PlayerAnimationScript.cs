using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    // Scripts
    public MovementScript movementScript;
    public Shift shift;
    public AttackWorld attackWorld;

    // Player Animator
    [SerializeField] Animator animator;

    private void Update()
    {
        IsMoving(); // If Player is moving, play running animation
        IsDashing(); // If Player is dashing, play dash animation
        IsAttacking();
    }

    private void IsMoving()
    {
        if (movementScript.moveDirection.magnitude >= 0.5f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void IsDashing()
    {
        animator.SetBool("isDash", shift.isShift);
    }

    private void IsAttacking()
    {
        animator.SetBool("isAttack", attackWorld.isAttacking);
        animator.SetBool("isLeft", attackWorld.isLeft);
    }
}
