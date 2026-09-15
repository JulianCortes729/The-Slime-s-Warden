using UnityEngine;


/// <summary>
/// Patrulla automática de enemigo: avanza constantemente y gira al detectar
/// una pared o un borde de plataforma usando raycasts de física.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.5f;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;

    private Rigidbody2D _rb;
    private float _facingDirection = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // El enemigo no debe rotar por física, solo por código
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // FIX #11: física + raycasts movidos a FixedUpdate para respetar el timestep de física.
    // En Update, el movimiento era visual-framerate-dependiente y los raycasts podían
    // ejecutarse en frames donde la física aún no había procesado las colisiones.
    private void FixedUpdate()
    {
        // 1. Sensores
        RaycastHit2D wall = Physics2D.Raycast(wallCheck.position, transform.right * _facingDirection, wallCheckDistance, obstacleLayer);
        RaycastHit2D ground = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, obstacleLayer);

        // 2. Cerebro: evalúa peligros
        if (wall.collider != null || ground.collider == null)
            Flip();

        // 3. Motor: movimiento via Rigidbody para interacción física correcta
        _rb.velocity = new Vector2(_facingDirection * speed, _rb.velocity.y);
    }

    private void Flip()
    {
        _facingDirection *= -1f;
        // Invertimos el flip visual del sprite
        //Vector3 scale = transform.localScale;
        //scale.x = _facingDirection;
        //transform.localScale = scale;

        // Movemos los puntos de chequeo al otro lado
        Vector3 wp = wallCheck.localPosition;
        wp.x = -wp.x;
        wallCheck.localPosition = wp;

        Vector3 gp = groundCheck.localPosition;
        gp.x = -gp.x;
        groundCheck.localPosition = gp;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Vector3 dir = transform.right * _facingDirection;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
            Gizmos.DrawSphere(wallCheck.position + dir * wallCheckDistance, 0.05f);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
            Gizmos.DrawSphere(groundCheck.position + Vector3.down * groundCheckDistance, 0.05f);
        }
    }
}
