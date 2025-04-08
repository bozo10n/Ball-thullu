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
    public float preferredDistanceFromPlayer = 12f;
    public float minDistanceFromPlayer = 8f;
    public float maxDistanceFromPlayer = 20f;

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

    public bool enableHoming = true;
    public float homingSpeed = 8f;            
    public float homingActivationDelay = 0.5f;
    public float homingMaxSpeed = 25f;        
    public AnimationCurve homingIntensityCurve = AnimationCurve.EaseInOut(0, 0.3f, 1, 1);
    private List<HomingProjectile> activeHomingProjectiles = new List<HomingProjectile>();

    public enum AttackPattern { Single, Burst, Volley, Spread }
    private AttackPattern currentPattern = AttackPattern.Single;
    private float patternSwitchTime = 10f;
    private float lastPatternSwitchTime;

    public int burstCount = 3;
    public float burstDelay = 0.2f;

    public int volleyCount = 5;
    public float volleyDelay = 0.1f;

    public int spreadCount = 3;
    public float spreadAngle = 30f;

    public bool rageMode = false;
    public float rageModeSpeedMultiplier = 1.5f;
    public float rageModeAttackRateMultiplier = 0.6f;
    public float rageModHomingStrengthMultiplier = 1.3f;

    private Vector3 formationCenter;
    private Vector3 targetPosition;

    private void Awake()
    {
        Instance = this;
        spinStartTime = Time.time;
        lastPatternSwitchTime = Time.time;

        if (player != null)
        {
            formationCenter = player.position + Vector3.up * heightAbovePlayer;
            targetPosition = CalculateTargetPosition();
        }

    }

    void Update()
    {
        if (player == null) return;

        CleanDestroyedBalls();

        FormChakra();

        float distanceToPlayer = Vector3.Distance(formationCenter, player.position);

        targetPosition = CalculateTargetPosition();

        formationCenter = Vector3.Lerp(formationCenter, targetPosition, Time.deltaTime * followSpeed);

        if (Time.time - lastPatternSwitchTime > patternSwitchTime)
        {
            SwitchAttackPattern();
            lastPatternSwitchTime = Time.time;
        }

        if (distanceToPlayer < preferredDistanceFromPlayer * 1.2f && !isOnCooldown)
        {
            ExecuteCurrentAttackPattern();
        }
    }

    Vector3 CalculateTargetPosition()
    {
        Vector3 directionFromPlayer = formationCenter - player.position;
        directionFromPlayer.y = 0; 

        float currentDistance = directionFromPlayer.magnitude;

        if (currentDistance < minDistanceFromPlayer)
        {
            directionFromPlayer = directionFromPlayer.normalized * preferredDistanceFromPlayer;
        }
        else if (currentDistance > maxDistanceFromPlayer)
        {
            directionFromPlayer = directionFromPlayer.normalized * preferredDistanceFromPlayer;
        }
        else
        {
            Quaternion rotation = Quaternion.AngleAxis(Time.deltaTime * 15f, Vector3.up);
            directionFromPlayer = rotation * directionFromPlayer.normalized * preferredDistanceFromPlayer;
        }

        Vector3 targetPos = player.position + directionFromPlayer;
        targetPos.y = player.position.y + heightAbovePlayer;

        return targetPos;
    }

    void SwitchAttackPattern()
    {
        int patternIndex = Random.Range(0, System.Enum.GetValues(typeof(AttackPattern)).Length);
        currentPattern = (AttackPattern)patternIndex;
    }

    void ExecuteCurrentAttackPattern()
    {
        switch (currentPattern)
        {
            case AttackPattern.Single:
                ShootSwarmBall();
                StartCoroutine(ShootCooldownCoroutine());
                break;

            case AttackPattern.Burst:
                StartCoroutine(BurstAttackCoroutine());
                break;

            case AttackPattern.Volley:
                StartCoroutine(VolleyAttackCoroutine());
                break;

            case AttackPattern.Spread:
                StartCoroutine(SpreadAttackCoroutine());
                break;
        }
    }

    public void addToSwarm(Transform ball)
    {
        if (ball == null || allSwarmBalls.Contains(ball))
        {
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
            rend = ball.AddComponent<MeshRenderer>();
            MeshFilter mf = ball.GetComponent<MeshFilter>();
            if (mf == null) mf = ball.AddComponent<MeshFilter>();
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mf.mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            Destroy(primitive);
            rend.material = new Material(Shader.Find("Standard"));
        }

        float randomShape = Random.value;
        rend.material.color = randomShape < 0.3f ? Color.black :
                               randomShape > 0.7f ? Color.red :
                               Color.black;
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

        Vector3 predictedPosition = PredictPlayerPosition();
        Vector3 shootDirection = (predictedPosition - chosenBall.position).normalized;
        shootDirection += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
        shootDirection.Normalize();

        ConfigureAndShootBall(chosenBall, shootDirection);
    }

    private Vector3 PredictPlayerPosition()
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            float predictionTime = Vector3.Distance(player.position, formationCenter) / shootVelocity;
            return player.position + playerRb.linearVelocity * predictionTime * 0.7f;
        }
        return player.position;
    }

    private void ConfigureAndShootBall(Transform ball, Vector3 direction)
    {
        if (enableHoming)
        {
            Rigidbody existingRb = ball.GetComponent<Rigidbody>();
            if (existingRb != null)
            {
                Destroy(existingRb);
            }
            SphereCollider sc = ball.GetComponent<SphereCollider>();
            if (sc == null) sc = ball.AddComponent<SphereCollider>();
            sc.isTrigger = true; 

            DamageOnCollision doc = ball.GetComponent<DamageOnCollision>();
            if (doc == null) ball.AddComponent<DamageOnCollision>();

            HomingProjectile homingComponent = ball.GetComponent<HomingProjectile>();
            if (homingComponent == null) homingComponent = ball.gameObject.AddComponent<HomingProjectile>();

            float homingSpeedValue = rageMode ? homingSpeed * rageModHomingStrengthMultiplier : homingSpeed;

            homingComponent.Initialize(
                player,
                homingSpeedValue,
                homingActivationDelay,
                homingIntensityCurve,
                homingMaxSpeed
            );

            ball.position += direction * shootVelocity * Time.deltaTime;

            activeHomingProjectiles.Add(homingComponent);
        }
        else
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = ball.AddComponent<Rigidbody>();
            }

            SphereCollider sc = ball.GetComponent<SphereCollider>();
            if (sc == null) sc = ball.AddComponent<SphereCollider>();
            sc.isTrigger = false;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;

            float currentVelocity = rageMode ? shootVelocity * rageModeSpeedMultiplier : shootVelocity;
            rb.AddForce(direction * currentVelocity, ForceMode.VelocityChange);

            DamageOnCollision doc = ball.GetComponent<DamageOnCollision>();
            if (doc == null) ball.AddComponent<DamageOnCollision>();
        }

        if (ball.gameObject != null)
        {
            Destroy(ball.gameObject, shootDestroyDelay);
        }
    }

    private IEnumerator BurstAttackCoroutine()
    {
        isOnCooldown = true;

        int actualBurstCount = Mathf.Min(burstCount, allSwarmBalls.Count);
        for (int i = 0; i < actualBurstCount; i++)
        {
            if (allSwarmBalls.Count == 0) break;
            ShootSwarmBall();
            yield return new WaitForSeconds(burstDelay);
        }

        float cooldownTime = rageMode ? shootCooldownDuration * rageModeAttackRateMultiplier : shootCooldownDuration;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    private IEnumerator VolleyAttackCoroutine()
    {
        isOnCooldown = true;

        int actualVolleyCount = Mathf.Min(volleyCount, allSwarmBalls.Count);
        for (int i = 0; i < actualVolleyCount; i++)
        {
            if (allSwarmBalls.Count == 0) break;

            Vector3 predictedPosition = PredictPlayerPosition();
            Vector3 baseDirection = (predictedPosition - formationCenter).normalized;

            int ballIndex = Random.Range(0, allSwarmBalls.Count);
            Transform chosenBall = allSwarmBalls[ballIndex];

            if (chosenBall == null)
            {
                allSwarmBalls.RemoveAt(ballIndex);
                continue;
            }

            allSwarmBalls.RemoveAt(ballIndex);
            orbitalAxes.Remove(chosenBall);
            orbitalSpeeds.Remove(chosenBall);
            orbitalOffsets.Remove(chosenBall);
            currentSpeedMultipliers.Remove(chosenBall);

            Vector3 shootDirection = baseDirection + new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f)
            ).normalized;

            ConfigureAndShootBall(chosenBall, shootDirection);
            yield return new WaitForSeconds(volleyDelay);
        }

        float cooldownTime = rageMode ? shootCooldownDuration * 1.2f * rageModeAttackRateMultiplier : shootCooldownDuration * 1.2f;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    private IEnumerator SpreadAttackCoroutine()
    {
        isOnCooldown = true;

        if (allSwarmBalls.Count >= spreadCount)
        {
            Vector3 baseDirection = (PredictPlayerPosition() - formationCenter).normalized;
            Vector3 rightVector = Vector3.Cross(baseDirection, Vector3.up).normalized;

            float angleStep = spreadAngle / (spreadCount - 1);
            float startAngle = -spreadAngle / 2;

            for (int i = 0; i < spreadCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 shootDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * baseDirection;

                int ballIndex = Random.Range(0, allSwarmBalls.Count);
                Transform chosenBall = allSwarmBalls[ballIndex];

                if (chosenBall == null)
                {
                    allSwarmBalls.RemoveAt(ballIndex);
                    continue;
                }

                allSwarmBalls.RemoveAt(ballIndex);
                orbitalAxes.Remove(chosenBall);
                orbitalSpeeds.Remove(chosenBall);
                orbitalOffsets.Remove(chosenBall);
                currentSpeedMultipliers.Remove(chosenBall);

                ConfigureAndShootBall(chosenBall, shootDirection);
            }
        }
        else
        {
            ShootSwarmBall();
        }

        float cooldownTime = rageMode ? shootCooldownDuration * 1.5f * rageModeAttackRateMultiplier : shootCooldownDuration * 1.5f;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    private IEnumerator ShootCooldownCoroutine()
    {
        isOnCooldown = true;
        float cooldownTime = rageMode ? shootCooldownDuration * rageModeAttackRateMultiplier : shootCooldownDuration;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    public void ActivateRageMode()
    {
        rageMode = true;
        patternSwitchTime *= 0.7f;
        formationRadius *= 1.2f;
        orbitalSpeed *= 1.3f;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(formationCenter, formationRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.position, formationCenter);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, preferredDistanceFromPlayer);

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);

            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        }
    }
}