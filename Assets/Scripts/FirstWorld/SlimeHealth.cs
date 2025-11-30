using UnityEngine;

public class SlimeHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;   // El Slime tiene 3 de vida
    private int currentHealth;

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
        Debug.Log("Slime muerto");
        // Puedes poner animación de muerte y luego Destroy
        Destroy(gameObject);
    }
}
