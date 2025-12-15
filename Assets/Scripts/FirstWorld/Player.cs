using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerAttackHitbox attackHitbox;
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;

    [Header("Sistema de Vida")]
    [SerializeField] private int maxHealth = 6;
    private int currentHealth;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;

    [Header("Configuración de Daño")]
    [SerializeField] private float invulnerabilityTime = 1.5f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Configuración de Respawn")]
    [SerializeField] private float tiempoAntesDespawn = 1f;
    [SerializeField] private bool fadeAlMorir = true;

    [Header("Configuración de Ataque")]
    [SerializeField] private float attackDuration = 0.3f;
    private bool isAttacking = false;

    [Header("Particles")]
    public ParticleSystem walkParticles;

    [Header("Audio")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float footstepInterval = 0.4f;

    private AudioSource audioSource;
    private float footstepTimer = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.down;
    private List<Image> heartImages = new List<Image>();

    private bool isInvulnerable = false;
    private bool isKnockedBack = false;
    private bool isDead = false;

    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.gravityScale = 0;

        posicionInicial = transform.position;

        currentHealth = maxHealth;
        CreateHearts();
        UpdateHearts();

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Asegurarse de que las partículas estén detenidas al inicio
        if (walkParticles != null)
        {
            walkParticles.Stop();
        }
    }

    void Update()
    {
        if (isDead || isKnockedBack) return;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }

        if (isAttacking) return;

        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");

        if (movimiento.magnitude > 0)
        {
            movimiento = movimiento.normalized;
            ultimaDireccion = movimiento;
        }

        // Controlar las partículas según el movimiento
        HandleWalkParticles();

        // Controlar sonido de pasos
        HandleFootsteps();

        UpdateHitboxPosition();

        if (animator != null)
        {
            animator.SetFloat("Horizontal", movimiento.x);
            animator.SetFloat("Vertical", movimiento.y);
            animator.SetFloat("Speed", movimiento.magnitude);

            if (animator.parameters.Length >= 5)
            {
                animator.SetFloat("LastHorizontal", ultimaDireccion.x);
                animator.SetFloat("LastVertical", ultimaDireccion.y);
            }
        }
    }

    void FixedUpdate()
    {
        if (!isKnockedBack && !isDead && !isAttacking)
        {
            rb.MovePosition(rb.position + movimiento * velocidadMovimiento * Time.fixedDeltaTime);
        }
    }

    void HandleWalkParticles()
    {
        if (walkParticles == null) return;

        if (movimiento.magnitude > 0)
        {
            if (!walkParticles.isPlaying)
            {
                walkParticles.Play();
            }
        }
        else
        {
            if (walkParticles.isPlaying)
            {
                walkParticles.Stop();
            }
        }
    }

    void HandleFootsteps()
    {
        if (movimiento.magnitude > 0)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                PlaySound(walkSound);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void UpdateHitboxPosition()
    {
        if (attackHitbox == null) return;

        Transform hitboxTransform = attackHitbox.transform;
        Vector3 offset = Vector3.zero;

        if (Mathf.Abs(ultimaDireccion.y) > Mathf.Abs(ultimaDireccion.x) && ultimaDireccion.y > 0)
        {
            offset = new Vector3(0f, 0.6f, 0f);
        }
        else if (Mathf.Abs(ultimaDireccion.y) > Mathf.Abs(ultimaDireccion.x) && ultimaDireccion.y < 0)
        {
            offset = new Vector3(0f, -0.6f, 0f);
        }
        else if (ultimaDireccion.x > 0)
        {
            offset = new Vector3(0.6f, 0f, 0f);
        }
        else if (ultimaDireccion.x < 0)
        {
            offset = new Vector3(-0.6f, 0f, 0f);
        }

        hitboxTransform.localPosition = offset;
    }

    void CreateHearts()
    {
        if (heartsContainer == null) return;

        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            Image heartImage = heart.GetComponent<Image>();
            heartImages.Add(heartImage);
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].sprite = emptyHeart;
            }
        }
    }

    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvulnerable || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        // Reproducir sonido de daño
        PlaySound(damageSound);

        StartCoroutine(InvulnerabilityCoroutine());
        StartCoroutine(KnockbackCoroutine(damageSource));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        CreateHearts();
        UpdateHearts();
    }

    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    IEnumerator KnockbackCoroutine(Vector2 damageSource)
    {
        isKnockedBack = true;

        if (walkParticles != null && walkParticles.isPlaying)
        {
            walkParticles.Stop();
        }

        Vector2 knockbackDirection = ((Vector2)transform.position - damageSource).normalized;
        rb.velocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;
        movimiento = Vector2.zero;

        // Reproducir sonido de muerte
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // Detener partículas al morir
        if (walkParticles != null && walkParticles.isPlaying)
        {
            walkParticles.Stop();
        }

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        if (fadeAlMorir)
        {
            float fadeTime = 0.5f;
            float elapsed = 0f;
            Color originalColor = spriteRenderer.color;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
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

        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;

        isDead = false;
        isInvulnerable = false;
        isKnockedBack = false;

        StartCoroutine(InvulnerabilidadRespawn());
    }

    IEnumerator InvulnerabilidadRespawn()
    {
        isInvulnerable = true;
        float tiempoProteccion = 2f;
        float elapsed = 0f;

        while (elapsed < tiempoProteccion)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1, collision.transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("HealthPickup"))
        {
            Heal(1);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("HeartContainer"))
        {
            IncreaseMaxHealth(1);
            Destroy(collision.gameObject);
        }
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        posicionInicial = newPosition;
        Debug.Log("Nueva posición de respawn establecida: " + newPosition);
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
            animator.SetFloat("LastHorizontal", ultimaDireccion.x);
            animator.SetFloat("LastVertical", ultimaDireccion.y);
        }

        if (attackHitbox != null)
        {
            attackHitbox.ActivateHitbox();
        }

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null)
        {
            attackHitbox.DeactivateHitbox();
        }

        isAttacking = false;
    }
}
