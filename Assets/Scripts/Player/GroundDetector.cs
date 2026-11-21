using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(PlayerBlackboard))]
public class GroundDetector : MonoBehaviour
{
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private PlayerBlackboard blackboard;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Por debajo de esta velocidad vertical el Brujo está cayendo: sobre un trampolín, va a rebotar
    private const float FallingSpeed = -0.1f;

    private readonly Collider2D[] _hits = new Collider2D[4]; // buffer reservado una vez: sin alloc por frame
    private Rigidbody2D _rb;

    private void Awake()
    {
        if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = Physics2D.queriesHitTriggers; // igual que el OverlapCircle con LayerMask de antes

        int count = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, filter, _hits);
        blackboard.isGrounded = CountsAsGround(TouchesSolidGround(count), blackboard.groundIgnoreTimer);
        if (blackboard.isGrounded)
        {
            blackboard.coyoteTimer = blackboard.coyoteTime; // Resetea el coyote timer al aterrizar
        }
    }

    // Basta con que uno de los contactos sea suelo de verdad
    private bool TouchesSolidGround(int count)
    {
        float verticalSpeed = _rb != null ? _rb.velocity.y : 0f;
        for (int i = 0; i < count; i++)
            if (IsSolidGround(_hits[i].GetComponentInParent<Trampoline>() != null, verticalSpeed)) return true;
        return false;
    }

    /// <summary>
    /// Un trampolín no es suelo mientras el Brujo cae sobre él: el círculo lo toca antes que la cápsula
    /// y un salto pedido saldría ahí, sin rebote. Parado encima (subió de lado o por una esquina y no rebotó)
    /// sí lo es: si no, se quedaría sin poder saltar. El suelo normal siempre cuenta.
    /// </summary>
    public static bool IsSolidGround(bool isTrampoline, float verticalSpeed)
    {
        return !isTrampoline || verticalSpeed >= FallingSpeed;
    }

    /// <summary>
    /// Regla del suelo: lo que toca el círculo cuenta como suelo salvo justo después de despegar,
    /// cuando los pies aún lo rozan (groundIgnoreTimer, que ponen el salto y el rebote).
    /// </summary>
    public static bool CountsAsGround(bool touchesSolidGround, float groundIgnoreTimer)
    {
        return touchesSolidGround && groundIgnoreTimer <= 0f;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
