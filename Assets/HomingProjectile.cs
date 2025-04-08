using UnityEngine;
using System.Collections.Generic;

public class HomingProjectile : MonoBehaviour
{
    private Transform target;
    private float homingSpeed;
    private float homingDelay;
    private AnimationCurve intensityCurve;
    private float maxSpeed;
    private float launchTime;
    private TrailRenderer trail;
    private bool hasHitTarget = false;
    [SerializeField] private float colliderRadius = 0.5f; 
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float targetHeightOffset = 0.75f; 

    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationForce = 2.5f;

    [SerializeField] private float leadTargetAmount = 0.2f; 
    [SerializeField] private float maxTargetDistance = 20f;
    [SerializeField] private float minApproachDistance = 0.5f;
    [SerializeField] private float targetingVariation = 0.5f; 

    private Vector3 targetOffset;
    private Vector3 targetLastPosition;
    private Vector3 targetVelocity = Vector3.zero;
    private static List<HomingProjectile> activeProjectiles = new List<HomingProjectile>();

    [SerializeField] private bool debugMode = false;

    public void Initialize(Transform targetTransform, float speed, float delay, AnimationCurve curve, float maxMoveSpeed)
    {
        target = targetTransform;
        homingSpeed = speed * 1.5f; 
        homingDelay = delay * 0.7f; 
        intensityCurve = curve;
        maxSpeed = maxMoveSpeed * 1.3f;
        launchTime = Time.time;
        hasHitTarget = false;
        targetLastPosition = target.position;

        activeProjectiles.Add(this);

        targetOffset = new Vector3(
            Random.Range(-targetingVariation, targetingVariation),
            targetHeightOffset,
            Random.Range(-targetingVariation, targetingVariation)
        );

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = colliderRadius;
            sphereCollider.isTrigger = true;
        }
        else
        {
            sphereCollider.radius = colliderRadius;
            sphereCollider.isTrigger = true;
        }

        trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.time = 0.3f;
            trail.widthMultiplier = 0.15f;
            Color originalColor = trail.startColor;
            Color enhancedColor = new Color(
                Mathf.Clamp01(originalColor.r * 1.2f),
                Mathf.Clamp01(originalColor.g * 1.2f),
                Mathf.Clamp01(originalColor.b * 1.2f),
                originalColor.a
            );
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(enhancedColor, 0.0f),
                    new GradientColorKey(originalColor, 0.5f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trail.colorGradient = gradient;
        }

        DamageOnCollision damageComponent = GetComponent<DamageOnCollision>();
        if (damageComponent == null)
        {
            damageComponent = gameObject.AddComponent<DamageOnCollision>();
            damageComponent.damageAmount = damage;
            damageComponent.destroyOnImpact = true;
            damageComponent.impactEffectPrefab = impactEffectPrefab;
        }
    }

    private void Update()
    {
        if (target == null || hasHitTarget) return;

        Vector3 targetDelta = target.position - targetLastPosition;
        targetVelocity = targetDelta / Time.deltaTime;
        targetLastPosition = target.position;

        float timeSinceLaunch = Time.time - launchTime;
        if (timeSinceLaunch < homingDelay) return;

        float homingProgress = Mathf.Clamp01((timeSinceLaunch - homingDelay) / 1.2f); 
        float intensity = intensityCurve.Evaluate(homingProgress);

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        float distanceMultiplier = Mathf.Clamp01(distanceToTarget / maxTargetDistance);
        float speedAdjust = Mathf.Lerp(0.8f, 1.2f, distanceMultiplier);

        Vector3 leadPosition = target.position + (targetVelocity * leadTargetAmount);
        Vector3 targetPosition = leadPosition + target.TransformDirection(targetOffset);

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        Vector3 separationDirection = CalculateSeparationForce();

        Vector3 finalDirection = (directionToTarget * 1.5f + separationDirection * 0.5f).normalized;

        float currentSpeed = homingSpeed * intensity * speedAdjust;
        if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;

        if (distanceToTarget < minApproachDistance)
        {
            currentSpeed *= distanceToTarget / minApproachDistance;
        }

        transform.position += finalDirection * currentSpeed * Time.deltaTime;

        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float wobbleAmount = 0.03f * intensity;
        Vector3 randomOffset = new Vector3(
            Mathf.Sin(Time.time * 8f) * wobbleAmount,
            Mathf.Cos(Time.time * 7f) * wobbleAmount,
            Mathf.Sin(Time.time * 6f) * wobbleAmount
        );
        transform.position += randomOffset * Time.deltaTime;

        if (debugMode && Application.isEditor)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.red);
            Debug.DrawRay(transform.position, finalDirection * 2f, Color.blue);
        }
    }

    private Vector3 CalculateSeparationForce()
    {
        Vector3 separationVector = Vector3.zero;

        activeProjectiles.RemoveAll(p => p == null);

        foreach (HomingProjectile otherProjectile in activeProjectiles)
        {
            if (otherProjectile == null || otherProjectile == this) continue;

            float distance = Vector3.Distance(transform.position, otherProjectile.transform.position);

            if (distance < separationRadius && distance > 0.01f)
            {
                Vector3 repulsionDirection = (transform.position - otherProjectile.transform.position).normalized;

                float repulsionStrength = separationForce * (1.0f - distance / separationRadius);

                separationVector += repulsionDirection * repulsionStrength;
            }
        }

        return separationVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitTarget) return;

        if (other.CompareTag("Player"))
        {
            hasHitTarget = true;
            Debug.Log("Projectile hit player!");
        }
        else if (other.CompareTag("Terrain") || other.CompareTag("Environment"))
        {
            hasHitTarget = true;
            Destroy(gameObject, 0.1f);
        }
    }

    private void OnDestroy()
    {
        if (activeProjectiles.Contains(this))
        {
            activeProjectiles.Remove(this);
        }
    }
}