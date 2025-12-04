using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Configuración")]
    [SerializeField] private float velocidad = 2f;

    private Transform destinoActual;

    void Start()
    {
        destinoActual = puntoB;
    }

    void Update()
    {
        // Mover plataforma
        transform.position = Vector2.MoveTowards(
            transform.position,
            destinoActual.position,
            velocidad * Time.deltaTime
        );

        // Si llega al destino → cambiar al otro punto
        if (Vector2.Distance(transform.position, destinoActual.position) < 0.1f)
        {
            destinoActual = (destinoActual == puntoA) ? puntoB : puntoA;
        }
    }
}
