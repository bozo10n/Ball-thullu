using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class ChakraController : MonoBehaviour
{
    public static ChakraController Instance;

    public Transform player;

    public Transform swarmCenter;
    public List<Transform> swarmBalls = new List<Transform>();

    private Dictionary<Transform, Vector3> orbitalAxes = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> orbitalSpeeds = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> orbitalOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> currentSpeedMultipliers = new Dictionary<Transform, float>();

    public float formationRadius = 1.3f;
    public float orbitalSpeed = 500f;
    public float cohesionStrength = 2f;
    public float separationRadius = 2f;
    public float separationForce = 2f;

    public float spinAccelerationTime = 2f;
    public AnimationCurve spinAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 sharedOrbitalAxis;
    private float spinStartTime;

    private bool isOnCooldown = false;
    private void Awake()
    {
        Instance = this;
        sharedOrbitalAxis = Vector3.up;
        spinStartTime = Time.time;
    }

    public void addToSwarm(Transform ball)
    {
        Debug.Log("Added to Swarm");
        if (!swarmBalls.Contains(ball))
        {
            swarmBalls.Add(ball);

            TrailRenderer tr = ball.gameObject.AddComponent<TrailRenderer>();
            tr.time = 0.5f;

            tr.widthMultiplier = 0.3f;
            tr.material = new Material(Shader.Find("Sprites/Default"));
            Renderer rend = ball.GetComponent<MeshRenderer>();

           

            // Color assignment
            float randomShape = Random.value;
            rend.material.color = randomShape < 0.3 ? Color.black :
                                   randomShape > 0.7 ? Color.white :
                                   Color.black;
            tr.colorGradient = CreateGradient(rend.material.color);
            // Orbital parameters
            Vector3 randomAxis = Random.insideUnitSphere.normalized;
            orbitalAxes[ball] = randomAxis;

            // Base orbital speed with variation
            float baseSpeed = orbitalSpeed * (1f + Random.Range(-0.15f, 0.15f));
            orbitalSpeeds[ball] = baseSpeed;

            // Initialize speed multiplier for smooth start
            currentSpeedMultipliers[ball] = 0f;

            // Random offset
            orbitalOffsets[ball] = Random.Range(0f, 360f);
        }
    }

    private Gradient CreateGradient(Color baseColor)
    {
        Gradient gradient = new Gradient();

        Color transparentColor = baseColor;
        transparentColor.a = 0;

        gradient.SetKeys(
            new GradientColorKey[] {
            new GradientColorKey(baseColor, 0.0f),
            new GradientColorKey(baseColor, 0.5f)
            },
            new GradientAlphaKey[] {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(0.0f, 1.0f)
            }
        );

        return gradient;
    }
    private void Update()
    {
        swarmBalls.RemoveAll(ball => ball == null);
        formChakra();
        Vector3 distanceToPlayer = player.position - swarmCenter.position;

        if (distanceToPlayer.magnitude < 8f && !isOnCooldown)
        {
            Debug.Log("Player in range");
            shootSwarmBall();
            StartCoroutine(ShootCooldown());
        }

    }

    private void formChakra()
    {
        // Calculate current spin acceleration progress
        float spinProgress = Mathf.Clamp01((Time.time - spinStartTime) / spinAccelerationTime);
        float accelerationFactor = spinAccelerationCurve.Evaluate(spinProgress);

        foreach (Transform swarmBall in swarmBalls)
        {
            if (swarmBall == null) { continue; }

            // Gradually increase speed multiplier
            currentSpeedMultipliers[swarmBall] = Mathf.Lerp(
                currentSpeedMultipliers[swarmBall],
                1f,
                Time.deltaTime / spinAccelerationTime
            );

            Vector3 baseOrbitalPosition = CalculateOrbitalPosition(swarmBall, accelerationFactor);

            // Separation force
            Vector3 separationMove = CalculateSeparationForce(swarmBall);

            // Cohesion force to maintain spherical formation
            Vector3 toCenter = swarmCenter.position - baseOrbitalPosition;
            Vector3 cohesionMove = toCenter.normalized * cohesionStrength;

            // Combine forces and apply movement
            Vector3 finalMove = (cohesionMove + separationMove) * Time.deltaTime;
            Vector3 newPosition = baseOrbitalPosition + finalMove;
            swarmBall.position = newPosition;
        }
    }

    private Vector3 CalculateOrbitalPosition(Transform swarmBall, float accelerationFactor)
    {
        Vector3 orbitAxis = orbitalAxes[swarmBall];
        float uniqueSpeed = orbitalSpeeds[swarmBall];
        float uniqueOffset = orbitalOffsets[swarmBall];

        // Apply speed multiplier and acceleration factor
        float angle = (Time.time * uniqueSpeed * currentSpeedMultipliers[swarmBall] * accelerationFactor + uniqueOffset) % 360f;

        Quaternion rotation = Quaternion.AngleAxis(angle, sharedOrbitalAxis);
        Vector3 orbitalOffset = rotation * (Vector3.forward * formationRadius);
        return swarmCenter.position + orbitalOffset;
    }

    private Vector3 CalculateSeparationForce(Transform ball)
    {
        Vector3 separationMove = Vector3.zero;
        foreach (Transform otherBall in swarmBalls)
        {
            if (otherBall == ball) continue;

            Vector3 separation = ball.position - otherBall.position;
            float distance = separation.magnitude;

            if (distance < separationRadius)
            {
                separationMove += separation.normalized / (distance + 0.1f) * separationForce;
            }
        }
        return separationMove;
    }

    private void shootSwarmBall()
    {
        if (swarmBalls.Count == 0) { return; }

        Transform chosenBall = swarmBalls[Random.Range(0, swarmBalls.Count)];
        swarmBalls.Remove(chosenBall);

        if (orbitalAxes.ContainsKey(chosenBall)) { orbitalAxes.Remove(chosenBall); }
        if (orbitalSpeeds.ContainsKey(chosenBall)) { orbitalSpeeds.Remove(chosenBall); }
        if (orbitalOffsets.ContainsKey(chosenBall)) { orbitalOffsets.Remove(chosenBall); }
        if (currentSpeedMultipliers.ContainsKey(chosenBall)) { currentSpeedMultipliers.Remove(chosenBall); }

        Vector3 shootDirection = (player.position - chosenBall.position).normalized;

        float shootVelocity = 18f;
        Vector3 velocity = shootDirection * shootVelocity;

        Rigidbody rb = chosenBall.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = chosenBall.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }
        rb.useGravity = true;
        chosenBall.gameObject.AddComponent<SphereCollider>();
        rb.isKinematic = false;

        rb.AddForce(shootDirection * shootVelocity, ForceMode.VelocityChange);

        Destroy(chosenBall.gameObject, 3f);


    }

    private IEnumerator ShootCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(0.8f);
        isOnCooldown = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(swarmCenter.position, 8f);
        Gizmos.color = Color.yellow;
    }
}