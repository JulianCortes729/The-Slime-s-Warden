using UnityEngine;

[RequireComponent(typeof(PlayerBlackboard))]
public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private PlayerBlackboard blackboard;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    // FIX #14: los string lookups en Animator se hacían cada frame.
    // Animator.StringToHash() convierte el string a int una sola vez en Awake.
    // El setter por hash es O(1) directo, sin búsqueda de tabla.
    private static readonly int HashHorizontalVelocity = Animator.StringToHash("HorizontalVelocity");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Flip del sprite según dirección de movimiento
        if (blackboard.moveInput > 0f)
            transform.localScale = Vector3.one;
        else if (blackboard.moveInput < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        // FIX #14: setters por hash en lugar de string — sin lookup por nombre cada frame
        animator.SetInteger(HashHorizontalVelocity, (int)Mathf.Abs(blackboard.moveInput));
        animator.SetBool(HashIsGrounded, blackboard.isGrounded);
    }

    public void StartInvulnerability()
    {
        animator.SetBool("IsInvulnerable", true);
    }

    public void StopInvulnerability()
    {
        animator.SetBool("IsInvulnerable", false);
    }
}
