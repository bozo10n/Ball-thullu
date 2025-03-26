using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public static SwarmController Instance;

    public Transform swirlCenter;
    public List<Transform> swarmBalls = new List<Transform>();

    [Header("Swirl Parameters")]
    public float swirlSpeed = 20f;
    public float attractionForce = 2f;
    public float initialDistanceThreshold = 10f;
    public float minDistance = 1f;

    [Header("Emergent Behavior Parameters")]
    public float separationForce = 1f;
    public float alignmentForce = 0.5f;
    public float cohesionForce = 0.3f;
    public float neighborRadius = 5f;

    private Dictionary<Transform, Vector3> orbitAxes = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> velocities = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        Instance = this;
    }

    public void addToSwarm(Transform ball)
    {
        if (!swarmBalls.Contains(ball))
        {
            swarmBalls.Add(ball);

            float randomShape = Random.Range(0, 20);

            if (randomShape < 7)
            {
                Renderer rend = ball.GetComponent<MeshRenderer>();
                rend.material.color = Color.black;
            }
            else if (randomShape > 10)
            {
                Renderer rend = ball.GetComponent<MeshRenderer>();
                rend.material.color = Color.white;
            }

            Vector3 randomAxis = new Vector3(Random.value, 1f, Random.value).normalized;
            orbitAxes[ball] = randomAxis;
            velocities[ball] = Vector3.zero;
        }
    }

    void Update()
    {
        foreach (Transform swarmBall in swarmBalls)
        {
            if (swarmBall == null) { continue; }
            // Boids-inspired emergent behavior
            Vector3 separation = CalculateSeparation(swarmBall);
            Vector3 alignment = CalculateAlignment(swarmBall);
            Vector3 cohesion = CalculateCohesion(swarmBall);

            // Center attraction
            Vector3 toCenter = swirlCenter.position - swarmBall.position;
            float distance = toCenter.magnitude;

            Vector3 centerAttraction = Vector3.zero;
            if (distance > minDistance)
            {
                Vector3 direction = toCenter.normalized;
                float force = Mathf.Lerp(0, attractionForce, Mathf.InverseLerp(initialDistanceThreshold, minDistance, distance));
                centerAttraction = direction * force;
            }

            // Combine behaviors with weights
            Vector3 totalForce = centerAttraction +
                                 separation * separationForce +
                                 alignment * alignmentForce +
                                 cohesion * cohesionForce;

            // Update velocity with damping
            velocities[swarmBall] = Vector3.Lerp(velocities[swarmBall], totalForce, Time.deltaTime * 5f);
            swarmBall.position += velocities[swarmBall] * Time.deltaTime;

            // Orbital rotation
            float adjustedSwirlSpeed = Mathf.Lerp(swirlSpeed * 0.5f, swirlSpeed, Mathf.InverseLerp(initialDistanceThreshold, minDistance, distance));
            swarmBall.RotateAround(swirlCenter.position, orbitAxes[swarmBall], adjustedSwirlSpeed * Time.deltaTime);
        }
    }

    Vector3 CalculateSeparation(Transform currentBall)
    {
        Vector3 separationMove = Vector3.zero;
        int nearbyBalls = 0;

        foreach (Transform otherBall in swarmBalls)
        {
            if (otherBall == currentBall) continue;

            float distance = Vector3.Distance(currentBall.position, otherBall.position);
            if (distance < neighborRadius)
            {
                Vector3 diff = currentBall.position - otherBall.position;
                separationMove += diff.normalized / distance;
                nearbyBalls++;
            }
        }

        return nearbyBalls > 0 ? separationMove / nearbyBalls : Vector3.zero;
    }

    Vector3 CalculateAlignment(Transform currentBall)
    {
        Vector3 alignmentMove = Vector3.zero;
        int nearbyBalls = 0;

        foreach (Transform otherBall in swarmBalls)
        {
            if (otherBall == currentBall) continue;

            float distance = Vector3.Distance(currentBall.position, otherBall.position);
            if (distance < neighborRadius)
            {
                alignmentMove += velocities[otherBall];
                nearbyBalls++;
            }
        }

        return nearbyBalls > 0 ? alignmentMove / nearbyBalls : Vector3.zero;
    }

    Vector3 CalculateCohesion(Transform currentBall)
    {
        Vector3 centerOfMass = Vector3.zero;
        int nearbyBalls = 0;

        foreach (Transform otherBall in swarmBalls)
        {
            if (otherBall == currentBall) continue;

            float distance = Vector3.Distance(currentBall.position, otherBall.position);
            if (distance < neighborRadius)
            {
                centerOfMass += otherBall.position;
                nearbyBalls++;
            }
        }

        if (nearbyBalls > 0)
        {
            centerOfMass /= nearbyBalls;
            return (centerOfMass - currentBall.position).normalized;
        }

        return Vector3.zero;
    }
}