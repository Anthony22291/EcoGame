using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private AudioClip attackSound;
    private AudioSource audioSource;

    private bool canDamage = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void ActivateHitbox()
    {
        canDamage = true;

        // Reproducir sonido de ataque
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void DeactivateHitbox()
    {
        canDamage = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDamage) return;

        if (collision.CompareTag("Enemy"))
        {
            SlimeHealth slimeHealth = collision.GetComponent<SlimeHealth>();
            if (slimeHealth != null)
            {
                slimeHealth.TakeDamage(attackDamage);
                Debug.Log("¡Slime golpeado!");
            }
        }
    }
}
