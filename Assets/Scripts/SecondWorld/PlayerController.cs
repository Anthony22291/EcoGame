using UnityEngine;

public class PlayerController : MonoBehaviour

{

    [Header("Movimiento")]

    public float speed = 6f;

    public float jumpForce = 13f;

    private Rigidbody2D rb;

    private Vector3 originalScale;

    private float moveInput;

    [Header("Coyote Time")]

    public float coyoteTime = 0.15f;

    private float coyoteCounter;

    [Header("Jump Buffer (permite saltar aunque aprietes un poco antes)")]

    public float jumpBufferTime = 0.15f;

    private float jumpBufferCounter;

    // Ground por colisiones

    private int groundContacts = 0;

    private bool IsGrounded => groundContacts > 0;

    private bool jumpPressed;

    void Awake()

    {

        rb = GetComponent<Rigidbody2D>();

        originalScale = transform.localScale;

    }

    void Update()

    {

        // Entrada horizontal

        moveInput = Input.GetAxisRaw("Horizontal");

        // Guardamos el input de salto (no saltamos aquí)

        if (Input.GetKeyDown(KeyCode.Space))

        {

            jumpPressed = true;

        }

    }

    void FixedUpdate()

    {

        HandleMovement();

        HandleTimers();

        HandleJump();

    }

    // -------------------- MOVIMIENTO --------------------

    void HandleMovement()

    {

        // Movimiento horizontal

        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        // Flip sin deformar

        if (moveInput != 0)

        {

            transform.localScale = new Vector3(

                Mathf.Sign(moveInput) * Mathf.Abs(originalScale.x),

                originalScale.y,

                originalScale.z

            );

        }

    }

    // -------------------- TIMERS (COYOTE + JUMP BUFFER) --------------------

    void HandleTimers()

    {

        // Coyote time: tiempo desde la última vez que tocaste el suelo

        if (IsGrounded)

            coyoteCounter = coyoteTime;

        else

            coyoteCounter -= Time.fixedDeltaTime;

        // Jump buffer: tiempo desde que apretaste salto

        if (jumpPressed)

        {

            jumpBufferCounter = jumpBufferTime;

            jumpPressed = false; // consumimos la señal, pero dejamos correr el timer

        }

        else

        {

            jumpBufferCounter -= Time.fixedDeltaTime;

        }

    }

    // -------------------- SALTO --------------------

    void HandleJump()

    {

        // Solo saltamos si:

        // - Apretaste space hace poco (jumpBufferCounter > 0)

        // - Todavía estás dentro del coyote time (coyoteCounter > 0)

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)

        {

            // Reseteamos la velocidad vertical para que el salto siempre sea limpio

            rb.velocity = new Vector2(rb.velocity.x, 0f);

            // Impulso hacia arriba

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // Consumimos ambos timers

            jumpBufferCounter = 0f;

            coyoteCounter = 0f;

        }

    }

    // -------------------- DETECCIÓN DE SUELO POR COLISIÓN --------------------

    private void OnCollisionEnter2D(Collision2D collision)

    {

        if (collision.collider.CompareTag("Ground"))

        {

            groundContacts++;

        }

    }

    private void OnCollisionExit2D(Collision2D collision)

    {

        if (collision.collider.CompareTag("Ground"))

        {

            groundContacts--;

        }

    }

}
