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
    [SerializeField] private float anguloVision = 90f; // Ángulo de visión en grados
    [SerializeField] private LayerMask capaObstaculos; // Para detectar paredes

    [Header("Configuración de Persecución")]
    [SerializeField] private float velocidadPersecucion = 4.5f;
    [SerializeField] private float distanciaAtaque = 1f;

    [Header("Debug Visual")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private Color colorVisionNormal = Color.yellow;
    [SerializeField] private Color colorVisionAlerta = Color.red;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform jugador;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Configurar Rigidbody
        rb.freezeRotation = true;
        rb.gravityScale = 0;

        // Buscar al jugador
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con tag 'Player'");
        }

        // Iniciar patrulla
        GenerarDestinoAleatorio();
    }

    void Update()
    {
        // Detectar al jugador
        jugadorDetectado = DetectarJugador();

        // Cambiar estado según detección
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
            // Volver a patrullar si pierde de vista al jugador
            if (estadoActual == EstadoEnemigo.Persiguiendo || estadoActual == EstadoEnemigo.Atacando)
            {
                estadoActual = EstadoEnemigo.Patrullando;
                GenerarDestinoAleatorio();
            }
        }

        // Ejecutar comportamiento según estado
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

        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        // Mover el enemigo
        rb.MovePosition(rb.position + movimiento * Time.fixedDeltaTime);
    }

    // ========== SISTEMA DE PATRULLAJE ==========
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
            // Llegó al destino, cambiar a espera
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
            // Terminar espera y generar nuevo destino
            estadoActual = EstadoEnemigo.Patrullando;
            GenerarDestinoAleatorio();
        }
    }

    void GenerarDestinoAleatorio()
    {
        // Generar un punto aleatorio alrededor de la posición actual
        Vector2 direccionAleatoria = Random.insideUnitCircle.normalized;
        float distanciaAleatoria = Random.Range(1f, distanciaMovimiento);
        destinoAleatorio = (Vector2)transform.position + direccionAleatoria * distanciaAleatoria;
    }

    // ========== SISTEMA DE PERSECUCIÓN ==========
    void PerseguirJugador()
    {
        if (jugador == null) return;

        Vector2 direccion = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        movimiento = direccion * velocidadPersecucion;
        ultimaDireccion = direccion;
    }

    void AtacarJugador()
    {
        // Detener movimiento y mirar al jugador
        movimiento = Vector2.zero;

        if (jugador != null)
        {
            ultimaDireccion = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        }

        // Aquí puedes añadir la lógica de ataque
        Debug.Log("¡Atacando al jugador!");
    }

    // ========== SISTEMA DE VISIÓN ==========
    bool DetectarJugador()
    {
        if (jugador == null) return false;

        Vector2 direccionAlJugador = (Vector2)jugador.position - (Vector2)transform.position;
        float distanciaAlJugador = direccionAlJugador.magnitude;

        // 1. Verificar si está dentro del rango
        if (distanciaAlJugador > rangoVision) return false;

        // 2. Verificar si está dentro del ángulo de visión
        float anguloAlJugador = Vector2.Angle(ultimaDireccion, direccionAlJugador);
        if (anguloAlJugador > anguloVision / 2f) return false;

        // 3. Verificar si hay obstáculos (Raycast)
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccionAlJugador.normalized,
            distanciaAlJugador,
            capaObstaculos
        );

        // Si el raycast golpea algo antes del jugador, no puede verlo
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            return false;
        }

        return true;
    }

    // ========== ANIMACIONES ==========
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

    // ========== VISUALIZACIÓN EN EDITOR ==========
    void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;

        // Color según estado
        Gizmos.color = jugadorDetectado ? colorVisionAlerta : colorVisionNormal;

        // Dibujar rango de visión
        Vector3 direccion3D = new Vector3(ultimaDireccion.x, ultimaDireccion.y, 0);

        // Dibujar línea central de visión
        Gizmos.DrawRay(transform.position, direccion3D * rangoVision);

        // Dibujar cono de visión
        float anguloMitad = anguloVision / 2f;
        Vector3 ladoIzquierdo = Quaternion.Euler(0, 0, anguloMitad) * direccion3D * rangoVision;
        Vector3 ladoDerecho = Quaternion.Euler(0, 0, -anguloMitad) * direccion3D * rangoVision;

        Gizmos.DrawRay(transform.position, ladoIzquierdo);
        Gizmos.DrawRay(transform.position, ladoDerecho);

        // Dibujar arco del cono
        Vector3 posicionAnterior = transform.position + ladoIzquierdo;
        for (int i = 1; i <= 20; i++)
        {
            float angulo = Mathf.Lerp(-anguloMitad, anguloMitad, i / 20f);
            Vector3 direccionArco = Quaternion.Euler(0, 0, angulo) * direccion3D * rangoVision;
            Vector3 puntoArco = transform.position + direccionArco;
            Gizmos.DrawLine(posicionAnterior, puntoArco);
            posicionAnterior = puntoArco;
        }

        // Dibujar destino de patrulla
        if (estadoActual == EstadoEnemigo.Patrullando)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destinoAleatorio, 0.3f);
            Gizmos.DrawLine(transform.position, destinoAleatorio);
        }

        // Dibujar distancia de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}