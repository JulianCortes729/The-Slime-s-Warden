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

    private void Awake()
    {
        if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
    }

    private void FixedUpdate()
    {
        // OverlapCircle con LayerMask devuelve Collider2D (no array) → sin alloc
        blackboard.isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        if (blackboard.isGrounded)
        {
            blackboard.coyoteTimer = blackboard.coyoteTime; // Resetea el coyote timer al aterrizar
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
