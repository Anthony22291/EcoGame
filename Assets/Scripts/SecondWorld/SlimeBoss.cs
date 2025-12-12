using UnityEngine;
using System.Collections;

public class SlimeBoss : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints; // Puntos para caminar
    [SerializeField] private SpriteRenderer spriteRenderer;

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

    [Header("Estados")]
    [SerializeField] private int attacksBeforeVulnerable = 3; // Ataques antes de ser vulnerable
    [SerializeField] private float vulnerableDuration = 5f; // Duración del estado vulnerable
    [SerializeField] private float warningDuration = 2f; // Tiempo de advertencia antes de volver a inmune
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color vulnerableColor = Color.red;

    private int attackCount = 0;
    private bool isVulnerable = false;
    private bool isInWarningPhase = false;

    // Estado de la máquina de estados
    private enum BossState { Patrol, PrepareJump, Jumping, Vulnerable, Warning }
    private BossState currentState;

    private Rigidbody2D rb;
    private Vector2 targetPosition;

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
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Patrol:
                PatrolBehavior();
                break;
            case BossState.PrepareJump:
                // Esperar preparación
                break;
            case BossState.Jumping:
                // Física maneja el salto
                break;
            case BossState.Vulnerable:
                // Solo espera, no se mueve
                break;
            case BossState.Warning:
                // Solo espera
                break;
        }
    }

    // ==================== PATRULLA ====================
    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        // Moverse hacia el punto de patrulla actual
        Vector2 direction = ((Vector2)patrolPoints[currentPatrolIndex].position - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * walkSpeed, rb.velocity.y);

        // Voltear sprite según dirección
        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        // Si llegó al punto, cambiar al siguiente
        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

            // Decidir si hacer ataque de salto
            if (Random.value > 0.5f)
            {
                StartCoroutine(PrepareJumpAttack());
            }
        }
    }

    // ==================== ATAQUE DE SALTO ====================
    IEnumerator PrepareJumpAttack()
    {
        currentState = BossState.PrepareJump;
        rb.velocity = Vector2.zero;

        // Volver al punto más cercano
        Transform closestPoint = GetClosestPatrolPoint();
        if (closestPoint != null)
        {
            float moveTime = 0f;
            Vector2 startPos = transform.position;

            while (moveTime < 1f && Vector2.Distance(transform.position, closestPoint.position) > 0.2f)
            {
                moveTime += Time.deltaTime * 2f;
                transform.position = Vector2.Lerp(startPos, closestPoint.position, moveTime);
                yield return null;
            }
        }

        // Preparación visual (puedes agregar animación aquí)
        yield return new WaitForSeconds(jumpPrepareTime);

        // Saltar hacia el jugador
        currentState = BossState.Jumping;
        Vector2 jumpDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(jumpDirection.x * 5f, jumpForce);
        isGrounded = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar cuando cae al suelo después del salto
        if (collision.gameObject.CompareTag("Ground") && currentState == BossState.Jumping)
        {
            isGrounded = true;
            rb.velocity = Vector2.zero;

            // Disparar pinchos
            ShootSpikes();

            // Incrementar contador de ataques
            attackCount++;

            // Verificar si debe entrar en estado vulnerable
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

        // Dispara pincho hacia la derecha
        GameObject spikeRight = Instantiate(spikePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbRight = spikeRight.GetComponent<Rigidbody2D>();
        if (rbRight != null)
            rbRight.velocity = Vector2.right * spikeSpeed;
        Destroy(spikeRight, 5f);

        // Dispara pincho hacia la izquierda
        GameObject spikeLeft = Instantiate(spikePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbLeft = spikeLeft.GetComponent<Rigidbody2D>();
        if (rbLeft != null)
            rbLeft.velocity = Vector2.left * spikeSpeed;
        Destroy(spikeLeft, 5f);
    }

    // ==================== ESTADO VULNERABLE ====================
    IEnumerator EnterVulnerableState()
    {
        currentState = BossState.Vulnerable;
        isVulnerable = true;
        rb.velocity = Vector2.zero;
        attackCount = 0;

        // Cambiar a color vulnerable
        spriteRenderer.color = vulnerableColor;

        // Esperar duración vulnerable
        yield return new WaitForSeconds(vulnerableDuration);

        // Si no le quitaron vida, entrar en fase de advertencia
        if (isVulnerable)
        {
            StartCoroutine(WarningPhase());
        }
    }

    IEnumerator WarningPhase()
    {
        currentState = BossState.Warning;
        isInWarningPhase = true;

        // Volver a color normal para advertir
        spriteRenderer.color = normalColor;

        yield return new WaitForSeconds(warningDuration);

        // Salir del estado vulnerable
        ExitVulnerableState();
    }

    void ExitVulnerableState()
    {
        isVulnerable = false;
        isInWarningPhase = false;
        spriteRenderer.color = normalColor;
        currentState = BossState.Patrol;
    }

    // ==================== SISTEMA DE DAÑO ====================
    public void TakeDamage(int damage)
    {
        // Solo puede recibir daño si está vulnerable y no en fase de advertencia
        if (!isVulnerable || isInWarningPhase) return;

        currentHealth -= damage;

        Debug.Log("Boss recibió daño. Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Salir del estado vulnerable inmediatamente
            StopAllCoroutines();
            ExitVulnerableState();
        }
    }

    void Die()
    {
        Debug.Log("¡Boss derrotado!");
        // Aquí puedes agregar animación de muerte, drops, etc.
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

    // Visualizar en editor
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
