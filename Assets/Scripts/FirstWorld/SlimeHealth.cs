using UnityEngine;

public class SlimeHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Death Particles")]
    [SerializeField] private ParticleSystem deathParticles; // Sistema de partículas de muerte

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Slime recibe daño: " + damage + " -> vida: " + currentHealth);

        // Aquí puedes reproducir animación de daño si quieres
        // if (animator != null) animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Slime muerto - iniciando muerte");

        if (deathParticles != null)
        {
            Debug.Log("Instanciando partículas en: " + transform.position);

            // Forzar posición Z a 0 para Unity 2D
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, 0);
            ParticleSystem particles = Instantiate(deathParticles, spawnPosition, Quaternion.identity);

            // Forzar reproducción por si acaso
            particles.Play();

            Debug.Log("Partículas creadas - isPlaying: " + particles.isPlaying);

            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }
        else
        {
            Debug.LogError("¡El prefab deathParticles NO está asignado!");
        }

        Destroy(gameObject);
    }

}
