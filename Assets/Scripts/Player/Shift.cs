using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shift : MonoBehaviour
{
    MovementScript moveScript;

    [Header("Shift Force")]
    bool canShift = true;
    [SerializeField] float shiftForce = 10f;
    [SerializeField] float shiftTime = 0.8f;

    [Header("Cooldown")]
    [SerializeField] float cooldownTime = 1.0f;
    
    void Start()
    {
        moveScript = GetComponent<MovementScript>();
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canShift == true)
        {  
            StartCoroutine(DashCoroutine());
        }
    }

    // Coroutine that dashes player based on dash time
    // After dashing, an X second cooldown is placed
    IEnumerator DashCoroutine()
    {
        canShift = false;

        float startTime = Time.time;

        while (Time.time < startTime + shiftTime)
        {
            moveScript.player.Move(transform.forward * shiftForce * Time.deltaTime);
            yield return null;
        }

        // After cooldown ends, player is able to dash again
        yield return new WaitForSeconds(cooldownTime);
        canShift = true;

    }
}
