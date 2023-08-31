using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    public MovementScript movementScript;
    public Shift shift;

    [SerializeField] Animator animator;

    private void Update()
    {
        IsMoving(); // If Player is moving
        IsDashing();
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
        Debug.Log(shift.isShift);
        animator.SetBool("isDash", shift.isShift);
    }
}
