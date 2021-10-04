using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Axolittle : MonoBehaviour
{
    private Animator ani;
    private NavMeshAgent agent;

    private Vector3 goal;

    private Timestamp walkTimeout = new Timestamp(10.0f);
    private Timestamp walkTs = new Timestamp(3.0f);
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;

        if (walkTs.Expired)
        {
            if (goal == Vector3.zero)
            {
                Vector3 randomPoint = pos + Random.insideUnitSphere * 5.0f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    goal = hit.position;
                    agent.SetDestination(goal);
                    ani.SetBool("Moving", true);
                }
            }
            else if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        goal = Vector3.zero;
                        walkTs.Reset(Random.Range(3.5f, 6.0f));
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
        walkTs.Reset(3.0f);
        ani.SetBool("Moving", false);
        
        ani.SetTrigger("Wave");
        Debug.Log($"{gameObject.name} is waving!");
        transform.LookAt(Camera.main.transform.position);
    }
}
