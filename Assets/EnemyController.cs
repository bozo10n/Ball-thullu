using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class SphereEnemy : MonoBehaviour
{
    [SerializeField]
    public Transform player;
    private NavMeshAgent agent;
    private Rigidbody rb;

    private Renderer rend;

    public bool walkPointSet = false;
    private Vector3 walkPoint;
    private float walkPointRange = 50f;
    public LayerMask ground;

    private bool alreadyAttacked = false;
    private float timeBetweenAttacks = 5f;

    private float attackDamage = 10f;

    [SerializeField]
    private float sightRange = 8f;
    [SerializeField]
    private float attackRange = 4f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rend = GetComponent<MeshRenderer>();
        rend.material.color = Color.black;
    }
    private void Update()
    {

        Vector3 distanceToPlayer = player.position - transform.position;

        if (distanceToPlayer.magnitude < sightRange && distanceToPlayer.magnitude > attackRange)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer.magnitude < sightRange && distanceToPlayer.magnitude < attackRange)
        {
            AttackPlayer();
        }
        else
        {
            Patroling();
        }

    }


    private void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(walkPoint, out hit, 2f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }


    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);


            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
            {
                Debug.Log("Raycast hit: " + hit.transform.name);
                Explode();
                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>() ?? hit.transform.GetComponentInParent<PlayerHealth>() ?? hit.transform.GetComponentInChildren<PlayerHealth>();

                if (playerHealth == null)
                {
                    Debug.LogError("player Health compontent is null");
                }

                if (playerHealth != null && (player.position - transform.position).magnitude < attackRange)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
                
            }
        }
    }

    private void ResetAttack()
    {
        StopAllCoroutines();
        rend.material.color = Color.black;
        alreadyAttacked = false;
    }

    private void Explode()
    {
        StartCoroutine(BlinkBeforeExplosion());
    }
    private IEnumerator BlinkBeforeExplosion()
    {
        float duration = 0f;
        float totalDuration = 3f;
        while (duration < totalDuration)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.06f);
            rend.material.color = Color.black;
            yield return new WaitForSeconds(0.06f);
            rend.material.color = Color.blue;
            yield return new WaitForSeconds(0.06f);
            duration += 1;
        }
        rend.material.color = Color.grey;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(walkPoint, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}