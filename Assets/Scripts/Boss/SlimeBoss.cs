using UnityEngine;
using System.Collections;

public class SlimeBoss : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints; // Puntos para caminar
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites del Slime")]
    [SerializeField] private Sprite idleSprite; // Sprite cuando está quieto
    [SerializeField] private Sprite walkSprite; // Sprite cuando se mueve

    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Header("Ataque de Salto")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpPrepareTime = 1f;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private float spikeSpeed = 5f;
    private bool isGrounded = true;

    [Header("Ataque Embestida")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private GameObject warningIcon;  // Icono de advertencia sobre el boss

    [Header("Ataque Spikes del Cielo")]
    [SerializeField] private GameObject fallingSpikePrefab;
    [SerializeField] private int spikesPerWave = 5;
    [SerializeField] private float spikeSpawnWidth = 8f;
    [SerializeField] private float spikeSpawnHeight = 6f;
    [SerializeField] private float timeBetweenSpikes = 0.2f;

    [Header("Estados de Vulnerabilidad")]
    [SerializeField] private int attacksBeforeVulnerable = 3;
    [SerializeField] private float vulnerableDuration = 5f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color vulnerableColor = Color.red;

    private int attackCount = 0;
    private bool isVulnerable = false;
    private bool isInWarningPhase = false;

    private enum BossState
    {
        Patrol,
        PrepareJump,
        Jumping,
        DashAttack,
        SpikeRain,
        Vulnerable,
        Warning
    }

    private BossState currentState;

    private Rigidbody2D rb;
    private Vector2 targetPosition;

    [Header("Puerta de Salida")]
    [SerializeField] private ExitDoor exitDoor;

    [Header("Música del Boss")]
    [SerializeField] private AudioSource bossMusicSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentState = BossState.Patrol;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.color = normalColor;

        if (patrolPoints.Length > 0)
            targetPosition = patrolPoints[currentPatrolIndex].position;

        if (warningIcon != null)
            warningIcon.SetActive(false);

        UpdateSprite(false, 0);
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Patrol:
                PatrolBehavior();
                break;
            case BossState.PrepareJump:
                break;
            case BossState.Jumping:
                break;
            case BossState.DashAttack:
                break;
            case BossState.SpikeRain:
                break;
            case BossState.Vulnerable:
                break;
            case BossState.Warning:
                break;
        }
    }

    // ==================== CAMBIO DE SPRITES ====================
    void UpdateSprite(bool isMoving, float directionX)
    {
        if (spriteRenderer == null) return;

        if (isMoving && walkSprite != null)
        {
            spriteRenderer.sprite = walkSprite;
        }
        else if (!isMoving && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        if (directionX != 0)
        {
            spriteRenderer.flipX = directionX > 0;
        }
    }

    // ==================== PATRULLA ====================
    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        Vector2 target = patrolPoints[currentPatrolIndex].position;
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        rb.velocity = new Vector2(direction.x * walkSpeed, rb.velocity.y);
        UpdateSprite(true, direction.x);

        if (Vector2.Distance(transform.position, target) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

            float r = Random.value;
            if (r < 0.33f)
            {
                StartCoroutine(PrepareJumpAttack());
            }
            else if (r < 0.66f)
            {
                StartCoroutine(DashAttack());
            }
            else
            {
                StartCoroutine(SpikeRainAttack());
            }
        }
    }

    // ==================== ATAQUE DE SALTO ====================
    IEnumerator PrepareJumpAttack()
    {
        currentState = BossState.PrepareJump;
        rb.velocity = Vector2.zero;
        UpdateSprite(false, 0);

        Transform closestPoint = GetClosestPatrolPoint();
        if (closestPoint != null)
        {
            float moveTime = 0f;
            Vector2 startPos = transform.position;
            Vector2 targetPos = closestPoint.position;

            while (moveTime < 1f && Vector2.Distance(transform.position, closestPoint.position) > 0.2f)
            {
                moveTime += Time.deltaTime * 2f;
                transform.position = Vector2.Lerp(startPos, targetPos, moveTime);

                float dirX = targetPos.x - transform.position.x;
                UpdateSprite(true, dirX);

                yield return null;
            }
        }

        UpdateSprite(false, 0);
        yield return new WaitForSeconds(jumpPrepareTime);

        currentState = BossState.Jumping;
        Vector2 jumpDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;

        UpdateSprite(false, jumpDirection.x);
        rb.velocity = new Vector2(jumpDirection.x * 5f, jumpForce);
        isGrounded = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && currentState == BossState.Jumping)
        {
            isGrounded = true;
            rb.velocity = Vector2.zero;
            UpdateSprite(false, 0);

            ShootSpikes();

            attackCount++;

            if (attackCount >= attacksBeforeVulnerable)
            {
                StartCoroutine(EnterVulnerableState());
            }
            else
            {
                currentState = BossState.Patrol;
            }
        }
    }

    void ShootSpikes()
    {
        if (spikePrefab == null) return;

        GameObject spikeRight = Instantiate(spikePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbRight = spikeRight.GetComponent<Rigidbody2D>();
        if (rbRight != null)
            rbRight.velocity = Vector2.right * spikeSpeed;
        Destroy(spikeRight, 5f);

        GameObject spikeLeft = Instantiate(spikePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbLeft = spikeLeft.GetComponent<Rigidbody2D>();
        if (rbLeft != null)
            rbLeft.velocity = Vector2.left * spikeSpeed;
        Destroy(spikeLeft, 5f);
    }

    // ==================== ATAQUE EMBESTIDA ====================
    IEnumerator DashAttack()
    {
        currentState = BossState.DashAttack;
        rb.velocity = Vector2.zero;

        float dirX = player.position.x - transform.position.x;
        if (dirX != 0)
            UpdateSprite(false, dirX);

        if (warningIcon != null)
            warningIcon.SetActive(true);

        yield return new WaitForSeconds(0.6f);

        if (warningIcon != null)
            warningIcon.SetActive(false);

        float dashDir = Mathf.Sign(dirX == 0 ? 1 : dirX);
        rb.velocity = new Vector2(dashDir * dashSpeed, 0f);
        UpdateSprite(true, dashDir);

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        UpdateSprite(false, 0);

        yield return new WaitForSeconds(dashCooldown);

        currentState = BossState.Patrol;
    }

    // ==================== ATAQUE SPIKES DEL CIELO ====================
    IEnumerator SpikeRainAttack()
    {
        currentState = BossState.SpikeRain;
        rb.velocity = Vector2.zero;
        UpdateSprite(false, 0);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < spikesPerWave; i++)
        {
            if (fallingSpikePrefab != null && player != null)
            {
                float randomX = player.position.x +
                                Random.Range(-spikeSpawnWidth * 0.5f, spikeSpawnWidth * 0.5f);
                Vector3 spawnPos = new Vector3(randomX,
                                               player.position.y + spikeSpawnHeight,
                                               0f);

                Instantiate(fallingSpikePrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(timeBetweenSpikes);
        }

        yield return new WaitForSeconds(1f);

        currentState = BossState.Patrol;
    }

    // ==================== ESTADO VULNERABLE ====================
    IEnumerator EnterVulnerableState()
    {
        currentState = BossState.Vulnerable;
        isVulnerable = true;
        rb.velocity = Vector2.zero;
        attackCount = 0;

        UpdateSprite(false, 0);
        spriteRenderer.color = vulnerableColor;

        yield return new WaitForSeconds(vulnerableDuration);

        if (isVulnerable)
        {
            StartCoroutine(WarningPhase());
        }
    }

    IEnumerator WarningPhase()
    {
        currentState = BossState.Warning;
        isInWarningPhase = true;

        UpdateSprite(false, 0);
        spriteRenderer.color = normalColor;

        yield return new WaitForSeconds(warningDuration);

        ExitVulnerableState();
    }

    void ExitVulnerableState()
    {
        isVulnerable = false;
        isInWarningPhase = false;
        spriteRenderer.color = normalColor;
        currentState = BossState.Patrol;
    }

    // ==================== DAÑO ====================
    public void TakeDamage(int damage)
    {
        if (!isVulnerable || isInWarningPhase) return;

        currentHealth -= damage;
        Debug.Log("Boss recibió daño. Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines();
            ExitVulnerableState();
        }
    }

    void Die()
    {
        Debug.Log("¡Boss derrotado!");

        if (bossMusicSource != null)
            bossMusicSource.Stop();

        if (exitDoor != null)
            exitDoor.ActivateDoor();

        Destroy(gameObject);
    }

    // ==================== UTILIDADES ====================
    Transform GetClosestPatrolPoint()
    {
        if (patrolPoints.Length == 0) return null;

        Transform closest = patrolPoints[0];
        float minDistance = Vector2.Distance(transform.position, closest.position);

        foreach (Transform point in patrolPoints)
        {
            float distance = Vector2.Distance(transform.position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = point;
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        foreach (Transform point in patrolPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.5f);
        }
    }
}