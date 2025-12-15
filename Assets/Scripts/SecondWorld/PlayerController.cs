using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // -----------------------------------------------------
    // MOVIMIENTO PLATAFORMA (TU SISTEMA ORIGINAL)
    // -----------------------------------------------------

    [Header("Movimiento")]
    public float speed = 6f;
    public float jumpForce = 13f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;

    private Vector3 originalScale;
    private float moveInput;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private bool jumpPressed;

    private int groundContacts = 0;
    private bool IsGrounded => groundContacts > 0;


    // -----------------------------------------------------
    // SISTEMA DE VIDA 
    // -----------------------------------------------------

    [Header("Sistema de Vida")]
    [SerializeField] private int maxHealth = 6;
    private int currentHealth;

    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;

    private List<Image> heartImages = new List<Image>();

    [Header("Daño - Invulnerabilidad")]
    [SerializeField] private float invulnerabilityTime = 1.5f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private bool isInvulnerable = false;
    private bool isKnockedBack = false;
    private bool isDead = false;

    [Header("Respawn")]
    [SerializeField] private float tiempoAntesDespawn = 1f;
    [SerializeField] private bool fadeAlMorir = true;

    private Vector3 posicionInicial;

    [Header("Ataque")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    // =====================================================
    // AUDIO COMPLETO
    // =====================================================
    [Header("Audio Clips")]
    [SerializeField] private AudioClip attackClip;  // Sonido de ataque
    [SerializeField] private AudioClip jumpClip;    // Sonido de salto
    [SerializeField] private AudioClip walkClip;    // Sonido de caminar
    [SerializeField] private AudioClip damageClip;  // Sonido de recibir daño
    [SerializeField] private AudioClip deathClip;   // Sonido de muerte

    [Header("Configuración de Pasos")]
    [SerializeField] private float footstepInterval = 0.3f; // Intervalo entre pasos
    private float footstepTimer = 0f;

    private AudioSource audioSource;

    // =====================================================
    // START
    // =====================================================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        originalScale = transform.localScale;

        posicionInicial = transform.position;

        currentHealth = maxHealth;
        CreateHearts();
        UpdateHearts();

        // Obtener o añadir AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    // =====================================================
    // UPDATE (Entrada de movimiento y animaciones)
    // =====================================================
    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        bool attackInput = Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0);

        if (attackInput && Time.time >= nextAttackTime && !isDead)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        UpdateAnimations();
        HandleFootsteps();
    }

    void Attack()
    {
        animator.SetTrigger("Attack");

        // Reproducir sonido de ataque
        PlaySound(attackClip);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Daño al boss
            SlimeBoss boss = enemy.GetComponent<SlimeBoss>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                continue;
            }

            // Daño a slimes normales
            EnemyAI_Plataforma slime = enemy.GetComponent<EnemyAI_Plataforma>();
            if (slime != null)
            {
                slime.TakeDamage(attackDamage);
            }
        }
    }



    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // =====================================================
    // FIXED UPDATE (Físicas)
    // =====================================================
    void FixedUpdate()
    {
        if (!isKnockedBack && !isDead)
            HandleMovement();

        HandleTimers();
        HandleJump();
    }


    // -----------------------------------------------------
    // MOVIMIENTO PLATAFORMA
    // -----------------------------------------------------
    void HandleMovement()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (moveInput != 0)
        {
            sr.flipX = moveInput < 0;

            // Mover AttackPoint según la dirección
            if (attackPoint != null)
            {
                float direction = moveInput < 0 ? -1f : 1f;
                attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x) * direction,
                                                         attackPoint.localPosition.y,
                                                         attackPoint.localPosition.z);
            }
        }
    }


    void UpdateAnimations()
    {
        float speedX = Mathf.Abs(moveInput);
        animator.SetFloat("Speed", speedX);

        animator.SetBool("IsJumping", !IsGrounded && rb.velocity.y > 0);
    }

    // -----------------------------------------------------
    // SONIDO DE PASOS
    // -----------------------------------------------------
    void HandleFootsteps()
    {
        bool isMoving = IsGrounded && Mathf.Abs(moveInput) > 0.1f;

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                PlaySound(walkClip);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }


    // -----------------------------------------------------
    // TIMERS (Coyote + JumpBuffer)
    // -----------------------------------------------------
    void HandleTimers()
    {
        if (IsGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressed = false;
        }
        else
            jumpBufferCounter -= Time.fixedDeltaTime;
    }


    // -----------------------------------------------------
    // SALTO
    // -----------------------------------------------------
    void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            coyoteCounter = 0f;
            jumpBufferCounter = 0f;

            // Reproducir sonido de salto
            PlaySound(jumpClip);
        }
    }


    // -----------------------------------------------------
    // SISTEMA DE VIDA
    // -----------------------------------------------------

    void CreateHearts()
    {
        foreach (Transform child in heartsContainer)
            Destroy(child.gameObject);

        heartImages.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            heartImages.Add(heart.GetComponent<Image>());
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Count; i++)
            heartImages[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
    }

    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        // Reproducir sonido de daño
        PlaySound(damageClip);

        StartCoroutine(InvulnerabilityCoroutine());
        StartCoroutine(KnockbackCoroutine(damageSource));

        if (currentHealth <= 0)
            Die();
    }


    // -----------------------------------------------------
    // INVULNERABILIDAD
    // -----------------------------------------------------
    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            sr.enabled = !sr.enabled;
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        sr.enabled = true;
        isInvulnerable = false;
    }


    // -----------------------------------------------------
    // KNOCKBACK
    // -----------------------------------------------------
    IEnumerator KnockbackCoroutine(Vector2 damageSource)
    {
        isKnockedBack = true;

        Vector2 dir = ((Vector2)transform.position - damageSource).normalized;
        rb.velocity = dir * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }


    // -----------------------------------------------------
    // MUERTE + RESPAWN
    // -----------------------------------------------------
    void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;

        // Reproducir sonido de muerte
        PlaySound(deathClip);

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        if (fadeAlMorir)
        {
            float t = 0f;
            Color baseColor = sr.color;

            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1, 0, t / 0.5f);
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                yield return null;
            }
        }

        yield return new WaitForSeconds(tiempoAntesDespawn);

        Respawn();
    }

    void Respawn()
    {
        transform.position = posicionInicial;

        currentHealth = maxHealth;
        UpdateHearts();

        sr.color = Color.white;

        isDead = false;
        isInvulnerable = false;
        isKnockedBack = false;

        StartCoroutine(InvulnerabilidadRespawn());
    }

    IEnumerator InvulnerabilidadRespawn()
    {
        isInvulnerable = true;

        float t = 0f;
        while (t < 2f)
        {
            t += 0.1f;
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        sr.enabled = true;
        isInvulnerable = false;
    }


    // -----------------------------------------------------
    // REPRODUCIR SONIDO
    // -----------------------------------------------------
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


    // -----------------------------------------------------
    // TRIGGERS
    // -----------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            groundContacts++;

        if (collision.collider.CompareTag("Enemy"))
            TakeDamage(1, collision.transform.position);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
            groundContacts--;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HealthPickup"))
        {
            Heal(1);
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("HeartContainer"))
        {
            IncreaseMaxHealth(1);
            Destroy(collision.gameObject);
        }
    }


    // -----------------------------------------------------
    // API
    // -----------------------------------------------------
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHearts();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        CreateHearts();
        UpdateHearts();
    }

    public void SetRespawnPosition(Vector3 newPos)
    {
        posicionInicial = newPos;
    }
}
