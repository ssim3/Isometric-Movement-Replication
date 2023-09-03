using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent agent;
    public Animator enemy;

    [SerializeField] Transform[] targets;
    int currentTarget = 0;
    Vector3 target;

    [Header("Wait")]
    [SerializeField] float waitTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy.SetBool("isWalking", true);
        UpdateDestination();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, target) < 1)
        {

            StartCoroutine(chill());
            UpdateTarget();
            UpdateDestination();
            
        }

    }

    // Moves enemy AI to updated target
    void UpdateDestination()
    {
        target = targets[currentTarget].position;
        agent.SetDestination(target);

    }

    // Updates the current target, and if enemyAi has traversed through all targets, reset.
    void UpdateTarget()
    {
        currentTarget++;

        if (currentTarget == targets.Length)
        {
            currentTarget = 0;
        }
    }


    // EnemyAI will stop at each waypoint for X amount of time
    IEnumerator chill()
    {
        enemy.SetBool("isWalking", false);
        float currentTime = Time.time;
        agent.isStopped = true;
        yield return new WaitForSeconds(waitTime);
        agent.isStopped = false;
        enemy.SetBool("isWalking", true);

    }
}
