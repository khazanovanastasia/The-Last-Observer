using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PrisonerAI : MonoBehaviour
{
    [Header("Movement")]
    public float minWalkDistance = 3f;
    public float maxWalkDistance = 8f;
    public float waitTime = 6f;
    public float stoppingDistance = 0.5f;

    [Header("Panopticon")]
    public Transform panopticonCenter;
    public float panopticonAvoidRadius = 15f;

    [Header("Door Block")]
    public float doorBlockPause = 2f;     
    public float retreatDistance = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private bool isWaiting = false;
    private bool isBlocked = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (panopticonCenter == null)
            panopticonCenter = GameObject.FindGameObjectWithTag("Panopticon")?.transform;

        SetNewTarget();
    }
    private void Update()
    {
        if (!isWaiting && !isBlocked
            && !agent.pathPending
            && agent.hasPath
            && agent.remainingDistance < stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }

        UpdateAnimation();
    }

    public void BlockedByDoor()
    {
        if (isBlocked) return;
        StartCoroutine(HandleDoorBlock());
    }

    private IEnumerator HandleDoorBlock()
    {
        isBlocked = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(doorBlockPause);

        agent.isStopped = false;
        isBlocked = false;

        SetTargetAwayFromDoor();
    }

    private IEnumerator WaitAndMove()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(waitTime);

        agent.isStopped = false;
        isWaiting = false;
        SetNewTarget();
    }

    private void SetNewTarget()
    {
        Vector3 point = GetRandomNavMeshPoint(transform.position, false);
        agent.SetDestination(point);
    }

    private void SetTargetAwayFromDoor()
    {
        Vector3 retreatOrigin = transform.position - transform.forward * retreatDistance;
        Vector3 point = GetRandomNavMeshPoint(retreatOrigin, true);
        agent.SetDestination(point);
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 origin, bool avoidPanopticon)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * Random.Range(minWalkDistance, maxWalkDistance);
            randomOffset.y = 0;
            Vector3 candidate = origin + randomOffset;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, maxWalkDistance, NavMesh.AllAreas))
                continue;

            if (avoidPanopticon && panopticonCenter != null)
            {
                float dist = Vector3.Distance(hit.position, panopticonCenter.position);
                if (dist < panopticonAvoidRadius) continue; 
            }

            return hit.position;
        }

        return transform.position; 
    }

    private void UpdateAnimation()
    {
        bool moving = !isWaiting && !isBlocked && agent.velocity.sqrMagnitude > 0.01f;
        animator.SetBool("isWalk", moving);

        if (moving && !audioSource.isPlaying)
            audioSource.Play();
        else if (!moving && audioSource.isPlaying)
            audioSource.Stop();
    }
}
