using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChakraController : MonoBehaviour
{
    public static ChakraController Instance;
    public Transform player;
    private bool isOnCooldown = false;

    public float heightAbovePlayer = 3.5f;
    public float followSpeed = 5f;

    public List<Transform> allSwarmBalls = new List<Transform>();
    private Dictionary<Transform, Vector3> orbitalAxes = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> orbitalSpeeds = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> orbitalOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, float> currentSpeedMultipliers = new Dictionary<Transform, float>();

    public float formationRadius = 1.3f;
    public float orbitalSpeed = 500f;
    public float separationRadius = 2f;
    public float separationForce = 2f;

    public float spinAccelerationTime = 2f;
    public AnimationCurve spinAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private float spinStartTime;
    private bool isShootingCooldown = false;
    public float shootCooldownDuration = 0.8f;
    public float shootVelocity = 18f;
    public float shootDestroyDelay = 3f;

    private Vector3 formationCenter;

    private void Awake()
    {
        Instance = this;
        spinStartTime = Time.time;


        if (player != null)
        {
            formationCenter = player.position + Vector3.up * heightAbovePlayer;
        }
        else
        {
            Debug.LogError("Player transform not assigned to ChakraController!");
            formationCenter = transform.position;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 distanceToPlayer = player.position - formationCenter;
        if (distanceToPlayer.magnitude < 8f && !isOnCooldown)
        {
            Debug.Log("Player in range");
            ShootSwarmBall();
            StartCoroutine(ShootCooldownCoroutine());
            CleanDestroyedBalls();

            FormChakra();
            return;
        }
        CleanDestroyedBalls();
        FormChakra();
        Vector3 targetCenter = player.position + Vector3.up * heightAbovePlayer;
        formationCenter = Vector3.Lerp(formationCenter, targetCenter, Time.deltaTime * followSpeed);

        
        
    }

    public void addToSwarm(Transform ball)
    {
        if (ball == null || allSwarmBalls.Contains(ball))
        {
            Debug.LogWarning("Cannot add null ball or duplicate ball.");
            return;
        }

        Debug.Log($"Adding {ball.name} to Swarm");
        allSwarmBalls.Add(ball);

        TrailRenderer tr = ball.gameObject.GetComponent<TrailRenderer>();
        if (tr == null) tr = ball.gameObject.AddComponent<TrailRenderer>();
        tr.time = 0.2f;
        tr.widthMultiplier = 0.1f;
        tr.material = new Material(Shader.Find("Sprites/Default"));

        Renderer rend = ball.GetComponent<MeshRenderer>();
        if (rend == null)
        {
            Debug.LogError($"Ball {ball.name} is missing a MeshRenderer!", ball.gameObject);
            rend = ball.AddComponent<MeshRenderer>();
            MeshFilter mf = ball.GetComponent<MeshFilter>();
            if (mf == null) mf = ball.AddComponent<MeshFilter>();
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mf.mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            Destroy(primitive);
            rend.material = new Material(Shader.Find("Standard"));
        }

        float randomShape = Random.value;
        rend.material.color = randomShape < 0.3f ? Color.black:
                               randomShape > 0.7f ? Color.yellow :
                               Color.white;
        tr.colorGradient = CreateGradient(rend.material.color);

        orbitalAxes[ball] = Random.insideUnitSphere.normalized;
        orbitalSpeeds[ball] = orbitalSpeed * (1f + Random.Range(-0.15f, 0.15f));
        orbitalOffsets[ball] = Random.Range(0f, 360f);
        currentSpeedMultipliers[ball] = 0f;
    }

    private Gradient CreateGradient(Color baseColor)
    {
        Gradient gradient = new Gradient();
        Color transparentColor = baseColor;
        transparentColor.a = 0;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(baseColor, 0.0f), new GradientColorKey(baseColor, 0.5f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        return gradient;
    }

    void CleanDestroyedBalls()
    {
        List<Transform> destroyedBalls = new List<Transform>();
        foreach (var ball in orbitalAxes.Keys)
        {
            if (ball == null) destroyedBalls.Add(ball);
        }

        foreach (var ball in destroyedBalls)
        {
            orbitalAxes.Remove(ball);
            orbitalSpeeds.Remove(ball);
            orbitalOffsets.Remove(ball);
            currentSpeedMultipliers.Remove(ball);
        }

        allSwarmBalls.RemoveAll(ball => ball == null);
    }

    void FormChakra()
    {
        float spinProgress = Mathf.Clamp01((Time.time - spinStartTime) / spinAccelerationTime);
        float accelerationFactor = spinAccelerationCurve.Evaluate(spinProgress);

        foreach (Transform swarmBall in allSwarmBalls)
        {
            if (swarmBall == null) continue;

            if (!orbitalAxes.ContainsKey(swarmBall)) continue;

            currentSpeedMultipliers[swarmBall] = Mathf.Lerp(
                currentSpeedMultipliers.ContainsKey(swarmBall) ? currentSpeedMultipliers[swarmBall] : 0f,
                1f,
                Time.deltaTime / (spinAccelerationTime + 0.01f)
            );

            Vector3 baseOrbitalPosition = CalculateOrbitalPosition(swarmBall, formationCenter, accelerationFactor);


            Vector3 separationMove = CalculateSeparationForce(swarmBall);

            Vector3 finalMove = separationMove * Time.deltaTime;
            Vector3 newPosition = baseOrbitalPosition + finalMove;
            swarmBall.position = Vector3.Lerp(swarmBall.position, newPosition, Time.deltaTime * 10f);
        }
    }

    Vector3 CalculateOrbitalPosition(Transform swarmBall, Vector3 orbitCenter, float accelerationFactor)
    {
        if (!orbitalAxes.ContainsKey(swarmBall) || !orbitalSpeeds.ContainsKey(swarmBall) ||
            !orbitalOffsets.ContainsKey(swarmBall) || !currentSpeedMultipliers.ContainsKey(swarmBall))
        {
            return swarmBall.position;
        }

        Vector3 orbitAxis = orbitalAxes[swarmBall];
        float uniqueSpeed = orbitalSpeeds[swarmBall];
        float uniqueOffset = orbitalOffsets[swarmBall];
        float speedMultiplier = currentSpeedMultipliers[swarmBall];

        float angle = (Time.time * uniqueSpeed * speedMultiplier * accelerationFactor + uniqueOffset) % 360f;
        Quaternion rotation = Quaternion.AngleAxis(angle, orbitAxis);
        Vector3 orbitalOffset = rotation * (Vector3.forward * formationRadius);

        return orbitCenter + orbitalOffset;
    }

    Vector3 CalculateSeparationForce(Transform ball)
    {
        Vector3 separationMove = Vector3.zero;

        foreach (Transform otherBall in allSwarmBalls)
        {
            if (otherBall == null || otherBall == ball) continue;

            Vector3 separation = ball.position - otherBall.position;
            float distanceSqr = separation.sqrMagnitude;

            if (distanceSqr < separationRadius * separationRadius && distanceSqr > 0.001f)
            {
                float distance = Mathf.Sqrt(distanceSqr);
                separationMove += (separation.normalized / (distance + 0.1f)) * separationForce;
            }
        }

        return separationMove;
    }

    void ShootSwarmBall()
    {
        if (allSwarmBalls.Count == 0 || player == null) return;

        int ballIndex = Random.Range(0, allSwarmBalls.Count);
        Transform chosenBall = allSwarmBalls[ballIndex];

        if (chosenBall == null)
        {
            allSwarmBalls.RemoveAt(ballIndex);
            return;
        }

        allSwarmBalls.RemoveAt(ballIndex);
        orbitalAxes.Remove(chosenBall);
        orbitalSpeeds.Remove(chosenBall);
        orbitalOffsets.Remove(chosenBall);
        currentSpeedMultipliers.Remove(chosenBall);

 
        Vector3 shootDirection = (player.position - chosenBall.position).normalized;
        shootDirection += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
        shootDirection.Normalize();

        Rigidbody rb = chosenBall.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = chosenBall.AddComponent<Rigidbody>();
        }

        SphereCollider sc = chosenBall.GetComponent<SphereCollider>();
        if (sc == null) sc = chosenBall.AddComponent<SphereCollider>();
        sc.isTrigger = false;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(shootDirection * shootVelocity, ForceMode.VelocityChange);

        DamageOnCollision doc = chosenBall.GetComponent<DamageOnCollision>();
        if (doc == null) chosenBall.AddComponent<DamageOnCollision>();

        if (chosenBall.gameObject != null)
        {
            Destroy(chosenBall.gameObject, shootDestroyDelay);
        }
    }

    private IEnumerator ShootCooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(shootCooldownDuration);
        isOnCooldown = false;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(formationCenter, formationRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.position, formationCenter);
        }
    }
}