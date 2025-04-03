using UnityEngine;
using UnityEngine.AI;

public class SwarmAgent : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent swarmAgent;
    [SerializeField]
    public Transform destination;
    private Rigidbody rb;
    private void Start()
    {
        
        swarmAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        swarmAgent.SetDestination(destination.position);
    }

    private void Update()
    {
        Vector3 distanceToDestination = destination.position - transform.position;
        if (distanceToDestination.magnitude < 2f)
        {
            
            swarmAgent.enabled = false;
            SwarmController.Instance.addToSwarm(this.transform);
        }
    }

}
