using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.gravityScale = 0;
    }

    void Update()
    {
        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");

        if (movimiento.magnitude > 0)
        {
            movimiento = movimiento.normalized;
            ultimaDireccion = movimiento;
        }

        if (animator != null)
        {
            animator.SetFloat("Horizontal", movimiento.x);
            animator.SetFloat("Vertical", movimiento.y);
            animator.SetFloat("Speed", movimiento.magnitude);

            if (animator.parameters.Length >= 5)
            {
                animator.SetFloat("LastHorizontal", ultimaDireccion.x);
                animator.SetFloat("LastVertical", ultimaDireccion.y);
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * velocidadMovimiento * Time.fixedDeltaTime);
    }
}