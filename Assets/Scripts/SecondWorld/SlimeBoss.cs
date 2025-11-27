using UnityEngine;
public class SlimeBoss : EnemyBase
{
    [Header("Boss Settings")]
    public float jumpForce = 12f;
    public float jumpCooldown = 4f;
    public float explosionRadius = 3f;
    public float damage = 2f;
    private float jumpTimer;
    private bool falling;
    protected override void Update()
    {
        base.Update();
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0 && isGrounded)
        {
            Jump();
            jumpTimer = jumpCooldown;
        }
        // detectar cuando cae
        if (!isGrounded) falling = true;
        if (falling && isGrounded)
        {
            falling = false;
            GroundExplosion();
        }
    }
    void Jump()
    {
        rb.velocity = new Vector2(0, jumpForce);
    }
    void GroundExplosion()
    {
        Debug.Log("Boss hizo explosión");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Jugador recibió daño de explosión del jefe");
                // Luego conectamos daño real al jugador
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}