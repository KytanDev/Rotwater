using UnityEngine;

public class FishBehaviour : MonoBehaviour
{
    [Header("Fish Properties")]
    [SerializeField] private float strength = 1f;
    [SerializeField] private float swimSpeed = 2f;
    [SerializeField] private float startingStamina = 100f;
    [SerializeField] private float currentStamina;
    public float Strength => strength;
    public float SwimSpeed => swimSpeed;

    private float baseStrength;
    private float baseSwimSpeed;
    private float baseSwimRange;

    public float CurrentStamina => currentStamina;
    public bool isExhausted => currentStamina <= 0f;

    [Header("Fighting Behaviour")]
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float swimRange = 5f;
    [SerializeField] private float fightReelSpeed = 3f;

    private bool isHooked;
    private float directionTimer;
    private int swimDirection = 1;
    private Vector3 fightRightDirection;
    public Vector3 RunDirection => fightRightDirection * swimDirection;
    private Vector3 fightCentre;
    private Transform playerTransform;

    private void Awake()
    {
        baseStrength = strength;
        baseSwimSpeed = swimSpeed;
        baseSwimRange = swimRange;
    }

    private void Update()
    {
        if (!isHooked) { return; }
        UpdateFight();
    }

    public void StartFight(Transform player, Vector3 hookPosition)
    {
        currentStamina = startingStamina;
        strength = baseStrength;
        swimRange = baseSwimRange;
        swimSpeed = baseSwimSpeed;

        transform.position = hookPosition;

        playerTransform = player;

        fightRightDirection = playerTransform.right;
        fightRightDirection.y = 0f;
        fightRightDirection.Normalize();

        fightCentre = transform.position;
        directionTimer = 0f;

        swimDirection = Random.value < 0.5f ? -1 : 1;

        currentStamina = startingStamina;
        isHooked = true;
    }

    private void UpdateFight()
    {
        directionTimer += Time.deltaTime;

        if (directionTimer >= directionChangeInterval)
        {
            swimDirection *= -1;
            directionTimer = 0f;
        }

        Vector3 rightDirection = fightRightDirection;

        Vector3 movement = rightDirection * swimDirection * swimSpeed * Time.deltaTime;

        Vector3 newPosition = transform.position + movement;
        Vector3 offset = newPosition - fightCentre;

        float lateralDistance = Vector3.Dot(offset, rightDirection);

        if (Mathf.Abs(lateralDistance) >= swimRange)
        {
            swimDirection *= -1;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    public void StopFight()
    {
        isHooked = false;
        playerTransform = null;
    }

    public void ReelTowardsPlayer()
    {
        if (!isHooked || playerTransform == null) {  return; }

        Vector3 direction = playerTransform.position - transform.position;

        direction.y = 0f;
        direction.Normalize();
    
        transform.position += direction * fightReelSpeed * Time.deltaTime;

        fightCentre += direction * fightReelSpeed * Time.deltaTime;
    }

    public void DrainStamina(float amount)
    {
        if (!isHooked || isExhausted) { return; }

        currentStamina -= amount * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0, startingStamina);

        StatsStaminaMultiplier();
    }

    public void StatsStaminaMultiplier()
    {
        float staminaMultiplier = currentStamina / startingStamina;

        strength = baseStrength * staminaMultiplier;
        swimSpeed = baseSwimSpeed * staminaMultiplier;
        swimRange = baseSwimRange * staminaMultiplier;
    }
}
