using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Configuración")]
    [SerializeField] private float velocidad = 2f;

    private Transform objetivoActual;

    private void Start()
    {
        objetivoActual = puntoB; // inicia moviéndose hacia B
    }

    void Update()
    {
        // Mover plataforma hacia el objetivo
        transform.position = Vector2.MoveTowards(
            transform.position,
            objetivoActual.position,
            velocidad * Time.deltaTime
        );

        // Cuando llega al punto, cambia al otro
        if (Vector2.Distance(transform.position, objetivoActual.position) < 0.1f)
        {
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;
        }
    }
}
