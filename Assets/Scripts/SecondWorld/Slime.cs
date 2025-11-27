using UnityEngine;

public class Slime : EnemyBase

{

    [Header("Slime Settings")]

    public float jumpForce = 6f;

    public float jumpCooldown = 2f;

    public float damage = 1f;

    private float jumpTimer;

    protected override void Update()

    {

        base.Update();

        jumpTimer -= Time.deltaTime;

        if (jumpTimer <= 0 && isGrounded)

        {

            JumpAtPlayer();

            jumpTimer = jumpCooldown;

        }

    }

    void JumpAtPlayer()

    {

        if (player == null) return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        rb.velocity = new Vector2(direction * 2f, jumpForce);

    }

    private void OnCollisionEnter2D(Collision2D collision)

    {

        if (collision.collider.CompareTag("Player"))

        {

            // Aquí luego conectamos al script del jugador

            Debug.Log("Slime golpeó al jugador");

        }

    }

}
