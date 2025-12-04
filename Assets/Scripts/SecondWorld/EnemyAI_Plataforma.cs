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

    private bool puedeAtacar = true;
    private float contadorAtaque = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Transform jugador;
    private Player playerScript;

    private enum Estado { Patrullando, Persiguiendo, Atacando }
    private Estado estadoActual = Estado.Patrullando;

    private Transform destinoActual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

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
    // PATRULLAJE ENTRE A ↔ B (SIN SIGN y SIN “flip loco”)
    // ============================================================
    void Patrullar()
    {
        float dx = destinoActual.position.x - transform.position.x;

        // Cambio de destino si llega
        if (Mathf.Abs(dx) < 0.1f)
        {
            destinoActual = destinoActual == puntoA ? puntoB : puntoA;
            dx = destinoActual.position.x - transform.position.x;
        }

        float dir = dx > 0 ? 1f : -1f;

        rb.velocity = new Vector2(dir * velocidadMovimiento, 0);

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

        rb.velocity = new Vector2(dir * velocidadMovimiento, 0);

        bool mirarIzq = dir < 0;
        sr.flipX = (mirarIzq != spriteMiraALaIzquierda);
    }

    // ============================================================
    // ATAQUE
    // ============================================================
    void AtacarJugador()
    {
        rb.velocity = Vector2.zero;

        if (jugador == null) return;

        float dx = jugador.position.x - transform.position.x;
        float dir = dx > 0 ? 1f : -1f;

        bool mirarIzq = dir < 0;
        sr.flipX = (mirarIzq != spriteMiraALaIzquierda);

        if (puedeAtacar)
        {
            playerScript.TakeDamage(danoPorContacto, transform.position);
            puedeAtacar = false;
        }
    }

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
