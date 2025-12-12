using UnityEngine;

public class EnemyAI_Plataforma : MonoBehaviour
{
    [Header("Movimiento Horizontal")]
    [SerializeField] private float velocidadMovimiento = 2f;
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Detección del Jugador")]
    [SerializeField] private float rangoDeteccion = 5f;
    [SerializeField] private float distanciaAtaque = 1f;

    [Header("Ataque")]
    [SerializeField] private int danoPorContacto = 1;
    [SerializeField] private float tiempoEntreAtaques = 1f;

    [Header("Sprite / Dirección Inicial")]
    [SerializeField] private bool spriteMiraALaIzquierda = true;

    // ============================================================
    // NUEVO: SALUD Y AUDIO DEL ENEMIGO
    // ============================================================
    [Header("Salud y Muerte")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Audio Slime")]
    [SerializeField] private AudioClip hitSoundClip;    // Sonido al recibir daño
    [SerializeField] private AudioClip deathSoundClip;  // Sonido de muerte (muerte.ogg)
    [SerializeField] private AudioClip attackSoundClip; // Sonido al atacar al jugador

    private AudioSource audioSource;
    private bool isDead = false;
    // ============================================================

    private bool puedeAtacar = true;
    private float contadorAtaque = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Transform jugador;
    private Player playerScript; // Script Player para el top-down. Si usas PlayerController, esto aún funcionaría.

    private enum Estado { Patrullando, Persiguiendo, Atacando }
    private Estado estadoActual = Estado.Patrullando;

    private Transform destinoActual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // NUEVO: Inicializar AudioSource y Salud
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        currentHealth = maxHealth;
        // FIN NUEVO

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        sr.flipX = spriteMiraALaIzquierda;

        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
            playerScript = objJugador.GetComponent<Player>(); // Puede ser Player o PlayerController
        }

        destinoActual = puntoB;
    }

    void Update()
    {
        if (isDead) return; // Detener comportamiento si está muerto

        if (!puedeAtacar)
        {
            contadorAtaque += Time.deltaTime;
            if (contadorAtaque >= tiempoEntreAtaques)
            {
                puedeAtacar = true;
                contadorAtaque = 0f;
            }
        }

        float distanciaJugador = jugador != null
            ? Vector2.Distance(transform.position, jugador.position)
            : Mathf.Infinity;

        if (distanciaJugador <= rangoDeteccion)
        {
            estadoActual = distanciaJugador <= distanciaAtaque
                ? Estado.Atacando
                : Estado.Persiguiendo;
        }
        else
        {
            estadoActual = Estado.Patrullando;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return; // Detener movimiento si está muerto

        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                break;

            case Estado.Persiguiendo:
                PerseguirJugador();
                break;

            case Estado.Atacando:
                AtacarJugador();
                break;
        }
    }

    // ============================================================
    // PATRULLAJE
    // ============================================================
    void Patrullar()
    {
        // ... (código Patrullar existente)
        float dx = destinoActual.position.x - transform.position.x;

        // Cambio de destino si llega
        if (Mathf.Abs(dx) < 0.1f)
        {
            destinoActual = destinoActual == puntoA ? puntoB : puntoA;
            dx = destinoActual.position.x - transform.position.x;
        }

        float dir = dx > 0 ? 1f : -1f;

        rb.velocity = new Vector2(dir * velocidadMovimiento, rb.velocity.y); // Usar rb.velocity.y para plataformas

        // Flip estable
        bool mirarIzq = dir < 0;
        sr.flipX = (mirarIzq != spriteMiraALaIzquierda);
    }

    // ============================================================
    // PERSECUCIÓN
    // ============================================================
    void PerseguirJugador()
    {
        if (jugador == null) return;

        float dx = jugador.position.x - transform.position.x;

        float dir = dx > 0 ? 1f : -1f;

        rb.velocity = new Vector2(dir * velocidadMovimiento, rb.velocity.y); // Usar rb.velocity.y

        bool mirarIzq = dir < 0;
        sr.flipX = (mirarIzq != spriteMiraALaIzquierda);
    }

    // ============================================================
    // ATAQUE
    // ============================================================
    void AtacarJugador()
    {
        rb.velocity = Vector2.zero; // Detener movimiento

        if (jugador == null) return;

        float dx = jugador.position.x - transform.position.x;
        float dir = dx > 0 ? 1f : -1f;

        bool mirarIzq = dir < 0;
        sr.flipX = (mirarIzq != spriteMiraALaIzquierda);

        if (puedeAtacar)
        {
            // NEW: Reproducir sonido de ataque
            if (audioSource != null && attackSoundClip != null)
            {
                audioSource.PlayOneShot(attackSoundClip);
            }

            // Asumiendo que el script del jugador es Player o PlayerController (ambos tienen TakeDamage)
            playerScript.TakeDamage(danoPorContacto, transform.position);
            puedeAtacar = false;
        }
    }

    // ============================================================
    // NUEVO: SISTEMA DE DAÑO
    // ============================================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Reproducir sonido de golpe
        if (audioSource != null && hitSoundClip != null)
        {
            audioSource.PlayOneShot(hitSoundClip);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false; // Desactivar colisión para que el jugador no reciba más daño
        sr.enabled = false; // O usar una animación de muerte

        // Reproducir sonido de muerte
        if (deathSoundClip != null)
        {
            // Usar PlayClipAtPoint para que el sonido se reproduzca incluso si el objeto se destruye
            AudioSource.PlayClipAtPoint(deathSoundClip, transform.position);
        }

        // Destruir el enemigo después de un pequeño retraso
        Destroy(gameObject, 0.5f);
    }

    // ============================================================
    // GIZMOS (Sin cambios)
    // ============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(puntoA.position, puntoB.position);
        }
    }
}