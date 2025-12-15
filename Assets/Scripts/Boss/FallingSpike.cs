using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;

    void Start()
    {
        // Destruir después de un tiempo si no toca nada
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si toca al player, hacerle daño
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
        // Si toca CUALQUIER otra cosa (suelo, paredes, etc.), destruirse
        else
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si toca al player, hacerle daño
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
        // Si toca CUALQUIER otra cosa (suelo, paredes, etc.), destruirse
        else
        {
            Destroy(gameObject);
        }
    }
}
