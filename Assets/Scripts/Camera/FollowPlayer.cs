using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 currentVelocity = Vector3.zero;

    public Transform target;

    [SerializeField] float smoothTime = 0.25f;

    private void Start()
    {
        offset = transform.position - target.position;
    }
    void Update()
    {
        var targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}
