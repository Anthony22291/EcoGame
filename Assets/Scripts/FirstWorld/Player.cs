using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
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
    [SerializeField] private float tiempoAntesDespawn = 1f; // Tiempo antes de reaparecer
    [SerializeField] private bool fadeAlMorir = true; // Efecto de desvanecimiento

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.down;
    private List<Image> heartImages = new List<Image>();

    private bool isInvulnerable = false;
    private bool isKnockedBack = false;
    private bool isDead = false;

    // Posición inicial del jugador
    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.gravityScale = 0;

        // Guardar la posición inicial
        posicionInicial = transform.position;

        currentHealth = maxHealth;
        CreateHearts();
        UpdateHearts();
    }

    void Update()
    {
        if (isDead || isKnockedBack) return;

        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");

        if (movimiento.magnitude > 0)
        {
            movimiento = movimiento.normalized;
            ultimaDireccion = movimiento;
        }

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
        if (!isKnockedBack && !isDead)
        {
            rb.MovePosition(rb.position + movimiento * velocidadMovimiento * Time.fixedDeltaTime);
        }
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

        Debug.Log("¡Jugador muerto! Reapareciendo...");

        // Iniciar el proceso de respawn
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        // Opcional: Efecto de desvanecimiento
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

        // Esperar antes de reaparecer
        yield return new WaitForSeconds(tiempoAntesDespawn);

        // Reaparecer en la posición inicial
        Respawn();
    }

    void Respawn()
    {
        // Restaurar posición
        transform.position = posicionInicial;

        // Restaurar vida completa
        currentHealth = maxHealth;
        UpdateHearts();

        // Restaurar estado visual
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;

        // Restaurar estados
        isDead = false;
        isInvulnerable = false;
        isKnockedBack = false;

        // Aplicar invulnerabilidad temporal al reaparecer
        StartCoroutine(InvulnerabilidadRespawn());

        Debug.Log("¡Jugador reaparecido!");
    }

    IEnumerator InvulnerabilidadRespawn()
    {
        isInvulnerable = true;
        float tiempoProteccion = 2f; // 2 segundos de invulnerabilidad al reaparecer
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

    // Método público para cambiar la posición de respawn (útil para checkpoints)
    public void SetRespawnPosition(Vector3 newPosition)
    {
        posicionInicial = newPosition;
        Debug.Log("Nueva posición de respawn establecida: " + newPosition);
    }
}