using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shift : MonoBehaviour
{
    MovementScript moveScript;
    
    [Header("Shift Force")]
    [SerializeField] float shiftForce = 10f;
    [SerializeField] float shiftTime = 0.8f;
    
    void Start()
    {
        moveScript = GetComponent<MovementScript>();
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(DashCoroutine());
        }

    }


    IEnumerator DashCoroutine()
    {
        float startTime = Time.time;
        while (Time.time < startTime + shiftTime)
        {
            moveScript.player.Move(transform.forward * shiftForce * Time.deltaTime);
            yield return null; 
        }
}
}
