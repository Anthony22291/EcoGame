using UnityEngine;
using UnityEngine.UI;
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float currentHealth;
    [Header("Vida UI")]
    public Canvas worldCanvas;
    public Image healthFill;
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.25f;
    public LayerMask groundLayer;
    public bool isGrounded;
    protected Rigidbody2D rb;
    protected Transform player;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        if (GameObject.FindGameObjectWithTag("Player") != null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    protected virtual void Update()
    {
        GroundCheck();
        UpdateHealthBar();
    }
    protected void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }
    public virtual void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
    protected void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;
    }
}