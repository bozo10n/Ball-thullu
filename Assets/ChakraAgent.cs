using UnityEngine;
using UnityEngine.AI;

public class ChakraAgent : MonoBehaviour
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

        if (ChakraController.Instance == null)
        {
            Debug.LogError("ChakraController instance is null!");
        }
    }

    private void Update()
    {
        Vector3 distanceToDestination = destination.position - transform.position;
        if (distanceToDestination.magnitude < 2f)
        {
            swarmAgent.enabled = false;
            ChakraController.Instance.addToSwarm(this.transform);
        }
    }

}
