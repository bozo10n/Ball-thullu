using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class SphereEnemy : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private Rigidbody rb;

    public float launchRange = 5f;
    public bool launched = false;

    public float launchForce = 20f;
    public float upwardForce = 10f;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Update()
    {
        if (launched)
        {
            if(IsGrounded())
            {
                ResetNavigation();
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= launchRange)
        {
            Launch();
        }
        else
        {
            agent.destination = target.position;
        }
    }

    private void Launch()
    {
        launched = true;
        agent.enabled = false;
        rb.isKinematic = false;
        Vector3 LaunchDirection = (target.position - transform.position).normalized;

        LaunchDirection.y = 0;

        rb.AddForce(LaunchDirection * launchForce + Vector3.up * upwardForce, ForceMode.Impulse);
    }

    private void ResetNavigation()
    {
        launched = false;
        
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        agent.enabled = true;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}   

