using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shift : MonoBehaviour
{
    Rigidbody rb;

    [Header("Shift Force")]
    [SerializeField] float shiftForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    
    void Update()
    {
        ShiftPlayer();
        Debug.Log(rb.velocity.magnitude);
    }

    void ShiftPlayer()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.AddForce(transform.forward * shiftForce, ForceMode.Impulse);
        }
    }
}
