using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Axolittle : MonoBehaviour
{
    private Animator ani;
    private NavMeshAgent agent;

    private Vector3 goal;
    private float collisionTime;

    private Timestamp walkTimeout = new Timestamp(10.0f);
    private Timestamp walkTs = new Timestamp(3.0f);

    private float nextWaveTime;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
        gameObject.AddComponent<Rigidbody>();
        var collider = gameObject.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        agent.speed = 0.5f;
        agent.radius = 0.4f;
        agent.stoppingDistance = 0.1f;

        nextWaveTime = Time.time + Random.Range(0f, 60f);
        NavMesh.avoidancePredictionTime = 5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        collisionTime = Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time - collisionTime > 3.0f)
        {
            goal = transform.position + (transform.position - other.transform.position);
            agent.SetDestination(goal);
            collisionTime = Time.time;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextWaveTime)
        {
            nextWaveTime = Time.time + Random.Range(30f, 90f);
            Wave();
        }
        
        var pos = transform.position;

        if (walkTs.Expired)
        {
            if (goal == Vector3.zero)
            {
                Vector3 randomPoint = pos + Random.insideUnitSphere * 20.0f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 3.0f, NavMesh.AllAreas))
                {
                    goal = hit.position;
                    agent.SetDestination(goal);
                    ani.SetBool("Moving", true);
                }
            }

            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        goal = Vector3.zero;
                        ani.SetBool("Moving", false);
                    }
                }
            }
        }
    }

    public void Wave()
    {
        agent.ResetPath();
        goal = Vector3.zero;
        walkTs.Reset(2.0f);
        ani.SetBool("Moving", false);
        
        ani.SetTrigger("Wave");
        // Debug.Log($"{gameObject.name} is waving!");
        transform.LookAt(Camera.main.transform.position);
    }
}
