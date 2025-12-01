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
    // Si tu slime mira a la izquierda por defecto → DEBE estar activado

    private bool puedeAtacar = true;
    private float contadorAtaque = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Transform jugador;
    private Player playerScript;

    private enum Estado { Patrullando, Persiguiendo, Atacando }
    private Estado estadoActual = Estado.Patrullando;

    private Transform destinoActual;
    private float direccion = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // Ajustar flip inicial según el sprite
        sr.flipX = spriteMiraALaIzquierda;

        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null)
        {
            jugador = objJugador.transform;
            playerScript = objJugador.GetComponent<Player>();
        }

        destinoActual = puntoB;
    }

    void Update()
    {
        // Cooldown de ataque
        if (!puedeAtacar)
        {
            contadorAtaque += Time.deltaTime;
            if (contadorAtaque >= tiempoEntreAtaques)
            {
                puedeAtacar = true;
                contadorAtaque = 0f;
            }
        }

        float distanciaJugador = jugador != null ?
            Vector2.Distance(transform.position, jugador.position) :
            Mathf.Infinity;

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
        switch (estadoActual)
        {
            case Estado.Patrullando: Patrullar(); break;
            case Estado.Persiguiendo: PerseguirJugador(); break;
            case Estado.Atacando: AtacarJugador(); break;
        }
    }

    // ============================
    // PATRULLAJE A ↔ B
    // ============================
    void Patrullar()
    {
        float distancia = Vector2.Distance(transform.position, destinoActual.position);

        if (distancia < 0.2f)
            destinoActual = destinoActual == puntoA ? puntoB : puntoA;

        direccion = Mathf.Sign(destinoActual.position.x - transform.position.x);

        rb.velocity = new Vector2(direccion * velocidadMovimiento, 0);

        // FIX DEL FLIP
        sr.flipX = (direccion < 0) != spriteMiraALaIzquierda;
    }

    // ============================
    // PERSECUCIÓN
    // ============================
    void PerseguirJugador()
    {
        if (jugador == null) return;

        direccion = Mathf.Sign(jugador.position.x - transform.position.x);

        rb.velocity = new Vector2(velocidadMovimiento * direccion, 0);

        // FIX DEL FLIP
        sr.flipX = (direccion < 0) != spriteMiraALaIzquierda;
    }

    // ============================
    // ATAQUE
    // ============================
    void AtacarJugador()
    {
        rb.velocity = Vector2.zero;

        if (jugador == null) return;

        direccion = Mathf.Sign(jugador.position.x - transform.position.x);

        // FIX DEL FLIP
        sr.flipX = (direccion < 0) != spriteMiraALaIzquierda;

        if (puedeAtacar)
        {
            playerScript.TakeDamage(danoPorContacto, transform.position);
            puedeAtacar = false;
        }
    }

    // ============================
    // GIZMOS
    // ============================
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
