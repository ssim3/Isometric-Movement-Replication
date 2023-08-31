using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class MovementScript : MonoBehaviour
{
    [Header("Player")]
    public CharacterController player;
    public Vector3 moveDirection;

    [Header("Speed")]
    [SerializeField] float speed = 20f;
    [SerializeField] float gravity = 20.0F;

    [Header("Rotation")]
    [SerializeField] float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    void Start()
    {
        player = GetComponent<CharacterController>();
    }


    void Update()
    {
        if (player.isGrounded)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            moveDirection = new Vector3(horizontal, 0, vertical).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                float angle = LookAndSmooth(moveDirection, turnSmoothVelocity, turnSmoothTime); // Get smoothed target angle
                moveDirection = SkewDirection(moveDirection); // Get skewed direction for isometric movement

                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        player.Move((moveDirection * speed * Time.deltaTime)); // Moves the player to the skewed direction


    }

    // Gets angle that we want to be facing, then smoothly interpolate between origin angle and target angle over time.
    // Vector3 direction : movement that user inputs
    // float turnSmoothVelocity : current velocity used internally by the function to calculate the smooth interpolation.
    // float turnSmoothTime : Approximate time it should take to reach the target angle
    
    float LookAndSmooth(Vector3 direction, float turnSmoothVelocity, float turnSmoothTime) 
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;                                                      
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle + 45f, ref turnSmoothVelocity, turnSmoothTime); 

        return angle;
    }

    Vector3 SkewDirection(Vector3 direction) // Fixes direction so movement follows isometric view
    {
        // Figure out wtf this means
        var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        Vector3 skewedDirection = matrix.MultiplyPoint3x4(direction).normalized;

        return skewedDirection;
    }

}
