using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ChakraSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject enemyPrefab;

    public float spawnRange = 15f;
    public int numberOfEnemies = 20;

    [SerializeField]
    public Transform destination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomPosition();
            GameObject chakraAgent = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            ChakraAgent swarmAgentController = chakraAgent.GetComponent<ChakraAgent>() ?? chakraAgent.GetComponentInParent<ChakraAgent>() ?? chakraAgent.GetComponentInChildren<ChakraAgent>();

            swarmAgentController.destination = destination;

            yield return new WaitForSeconds(0.1f);
        }
    }
    private Vector3 GetRandomPosition()
    {
        float random_x = Random.Range(-spawnRange, spawnRange);
        float random_z = Random.Range(-spawnRange, spawnRange);
        Vector3 newPosition = new Vector3(random_x, 0, random_z) + transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return newPosition;
    }

   
}
