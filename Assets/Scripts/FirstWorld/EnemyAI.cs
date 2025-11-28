using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [SerializeField] private float tiempoEsperaMin = 1f;
    [SerializeField] private float tiempoEsperaMax = 3f;
    [SerializeField] private float distanciaMovimiento = 3f;

    [Header("Configuración de Visión")]
    [SerializeField] private float rangoVision = 5f;
    [SerializeField] private float anguloVision = 90f;
    [SerializeField] private LayerMask capaObstaculos;

    [Header("Configuración de Persecución")]
    [SerializeField] private float velocidadPersecucion = 4.5f;
    [SerializeField] private float distanciaAtaque = 1f;

    [Header("Configuración de Daño")]
    [SerializeField] private int danoPorContacto = 1;
    [SerializeField] private float tiempoEntreAtaques = 1f;

    [Header("Debug Visual")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private Color colorVisionNormal = Color.yellow;
    [SerializeField] private Color colorVisionAlerta = Color.red;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform jugador;
    private Player playerScript;

    // Estados del enemigo
    private enum EstadoEnemigo { Patrullando, Esperando, Persiguiendo, Atacando }
    private EstadoEnemigo estadoActual = EstadoEnemigo.Patrullando;

    // Variables de movimiento
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.down;
    private Vector2 destinoAleatorio;
    private float tiempoEspera;
    private float contadorEspera;

    // Variables de visión
    private bool jugadorDetectado = false;

    // Variables de ataque
    private float contadorAtaque = 0f;
    private bool puedeAtacar = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.gravityScale = 0;

        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
            playerScript = objetoJugador.GetComponent<Player>();
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con tag 'Player'");
        }

        GenerarDestinoAleatorio();
    }

    void Update()
    {
        // Actualizar cooldown de ataque
        if (!puedeAtacar)
        {
            contadorAtaque += Time.deltaTime;
            if (contadorAtaque >= tiempoEntreAtaques)
            {
                puedeAtacar = true;
                contadorAtaque = 0f;
            }
        }

        jugadorDetectado = DetectarJugador();

        if (jugadorDetectado)
        {
            float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

            if (distanciaAlJugador <= distanciaAtaque)
            {
                estadoActual = EstadoEnemigo.Atacando;
            }
            else
            {
                estadoActual = EstadoEnemigo.Persiguiendo;
            }
        }
        else
        {
            if (estadoActual == EstadoEnemigo.Persiguiendo || estadoActual == EstadoEnemigo.Atacando)
            {
                estadoActual = EstadoEnemigo.Patrullando;
                GenerarDestinoAleatorio();
            }
        }

        switch (estadoActual)
        {
            case EstadoEnemigo.Patrullando:
                Patrullar();
                break;
            case EstadoEnemigo.Esperando:
                Esperar();
                break;
            case EstadoEnemigo.Persiguiendo:
                PerseguirJugador();
                break;
            case EstadoEnemigo.Atacando:
                AtacarJugador();
                break;
        }

        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * Time.fixedDeltaTime);
    }

    void Patrullar()
    {
        Vector2 direccion = (destinoAleatorio - (Vector2)transform.position).normalized;
        float distancia = Vector2.Distance(transform.position, destinoAleatorio);

        if (distancia > 0.2f)
        {
            movimiento = direccion * velocidadMovimiento;
            ultimaDireccion = direccion;
        }
        else
        {
            estadoActual = EstadoEnemigo.Esperando;
            tiempoEspera = Random.Range(tiempoEsperaMin, tiempoEsperaMax);
            contadorEspera = 0f;
        }
    }

    void Esperar()
    {
        movimiento = Vector2.zero;
        contadorEspera += Time.deltaTime;

        if (contadorEspera >= tiempoEspera)
        {
            estadoActual = EstadoEnemigo.Patrullando;
            GenerarDestinoAleatorio();
        }
    }

    void GenerarDestinoAleatorio()
    {
        Vector2 direccionAleatoria = Random.insideUnitCircle.normalized;
        float distanciaAleatoria = Random.Range(1f, distanciaMovimiento);
        destinoAleatorio = (Vector2)transform.position + direccionAleatoria * distanciaAleatoria;
    }

    void PerseguirJugador()
    {
        if (jugador == null) return;

        Vector2 direccion = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        movimiento = direccion * velocidadPersecucion;
        ultimaDireccion = direccion;
    }

    void AtacarJugador()
    {
        movimiento = Vector2.zero;

        if (jugador != null)
        {
            ultimaDireccion = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        }
    }

    bool DetectarJugador()
    {
        if (jugador == null) return false;

        Vector2 direccionAlJugador = (Vector2)jugador.position - (Vector2)transform.position;
        float distanciaAlJugador = direccionAlJugador.magnitude;

        if (distanciaAlJugador > rangoVision) return false;

        float anguloAlJugador = Vector2.Angle(ultimaDireccion, direccionAlJugador);
        if (anguloAlJugador > anguloVision / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccionAlJugador.normalized,
            distanciaAlJugador,
            capaObstaculos
        );

        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            return false;
        }

        return true;
    }

    void ActualizarAnimaciones()
    {
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movimiento.normalized.x);
            animator.SetFloat("Vertical", movimiento.normalized.y);
            animator.SetFloat("Speed", movimiento.magnitude);

            if (animator.parameters.Length >= 5)
            {
                animator.SetFloat("LastHorizontal", ultimaDireccion.x);
                animator.SetFloat("LastVertical", ultimaDireccion.y);
            }
        }
    }

    // ========== SISTEMA DE DAÑO ==========

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && puedeAtacar)
        {
            HacerDanoAlJugador();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && puedeAtacar)
        {
            HacerDanoAlJugador();
        }
    }

    void HacerDanoAlJugador()
    {
        if (playerScript != null && puedeAtacar)
        {
            playerScript.TakeDamage(danoPorContacto, transform.position);
            puedeAtacar = false;
            contadorAtaque = 0f;

            Debug.Log("¡Slime atacó al jugador!");
        }
    }

    void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;

        Gizmos.color = jugadorDetectado ? colorVisionAlerta : colorVisionNormal;

        Vector3 direccion3D = new Vector3(ultimaDireccion.x, ultimaDireccion.y, 0);

        Gizmos.DrawRay(transform.position, direccion3D * rangoVision);

        float anguloMitad = anguloVision / 2f;
        Vector3 ladoIzquierdo = Quaternion.Euler(0, 0, anguloMitad) * direccion3D * rangoVision;
        Vector3 ladoDerecho = Quaternion.Euler(0, 0, -anguloMitad) * direccion3D * rangoVision;

        Gizmos.DrawRay(transform.position, ladoIzquierdo);
        Gizmos.DrawRay(transform.position, ladoDerecho);

        Vector3 posicionAnterior = transform.position + ladoIzquierdo;
        for (int i = 1; i <= 20; i++)
        {
            float angulo = Mathf.Lerp(-anguloMitad, anguloMitad, i / 20f);
            Vector3 direccionArco = Quaternion.Euler(0, 0, angulo) * direccion3D * rangoVision;
            Vector3 puntoArco = transform.position + direccionArco;
            Gizmos.DrawLine(posicionAnterior, puntoArco);
            posicionAnterior = puntoArco;
        }

        if (estadoActual == EstadoEnemigo.Patrullando)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destinoAleatorio, 0.3f);
            Gizmos.DrawLine(transform.position, destinoAleatorio);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}