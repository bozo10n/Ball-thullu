using System.Collections.Generic;
using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public static SwarmController Instance;
    public Transform swirlCenter;
    public List<Transform> swarmBalls = new List<Transform>();
    public float swirlSpeed = 20f;
    public float attractionForce = 2f;
    public float initialDistanceThreshold = 10f;
    public float minDistance = 1f;

    private Dictionary<Transform, Vector3> orbitAxes = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    public void addToSwarm(Transform ball)
    {
        if (!swarmBalls.Contains(ball))
        {
            swarmBalls.Add(ball);

            Vector3 randomAxis = new Vector3(Random.value, 1f, Random.value).normalized;
            orbitAxes[ball] = randomAxis;
        }
    }

    void Update()
    {
        foreach (Transform swarmBall in swarmBalls)
        {
            if (swarmBall == null) { continue; }

            Vector3 toCenter = swirlCenter.position - swarmBall.position;
            float distance = toCenter.magnitude;

            if (distance > minDistance)
            {
                Vector3 direction = toCenter.normalized;
                float force = Mathf.Lerp(0, attractionForce, Mathf.InverseLerp(initialDistanceThreshold, minDistance, distance));
                swarmBall.position += direction * force * Time.deltaTime;
            }

            float adjustedSwirlSpeed = Mathf.Lerp(swirlSpeed * 0.5f, swirlSpeed, Mathf.InverseLerp(initialDistanceThreshold, minDistance, distance));
            swarmBall.RotateAround(swirlCenter.position, orbitAxes[swarmBall], adjustedSwirlSpeed * Time.deltaTime);
        }
    }
}
