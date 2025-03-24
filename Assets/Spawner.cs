using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public GameObject enemyPrefab;

    public float spawnRange = 50f;
    public int numberOfEnemies = 20;

    public Transform player;

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
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            EnemyController enemyController = enemy.GetComponent<EnemyController>() ?? enemy.GetComponentInParent <EnemyController>() ?? enemy.GetComponentInChildren<EnemyController>();

            enemyController.player = player;

            yield return new WaitForSeconds(2f);
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
