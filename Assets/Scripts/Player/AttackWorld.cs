using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackWorld : MonoBehaviour
{
    public bool isAttacking;

    [Header("Cooldown")]
    float attackCooldown = 0.5f;

    [Header("Attack Mechanics")]
    float attackTime = 1.5f;
    public MovementScript movementScript;
    public bool isLeft;

    float originalSpeed;

    // Start is called before the first frame update
    void Start()
    {
        isAttacking = false;
        isLeft = true;

        originalSpeed = movementScript.speed;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Attacking");
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        isLeft = !isLeft;

        float startTime = Time.time;

        while (Time.time < startTime + attackTime)
        {
            yield return null;
        }

        // Stops animation
        isAttacking = false;

    }
}
