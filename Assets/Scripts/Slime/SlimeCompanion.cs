using UnityEngine;

/// <summary>
/// Compañero que se mueve junto al jugador en X usando SmoothDamp,
/// y busca el suelo bajo sí mismo con un raycast para "posarse" en plataformas.
/// </summary>
public class SlimeCompanion : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothTimeX = 0.15f;
    [SerializeField] private float smoothTimeY = 0.1f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private float yOffset = 0.5f;
    [SerializeField] private float xOffset = 0.5f;
    [SerializeField] private float playerFeetOffset = 0.8f;
    // en SlimeCompanion, agregar a [Header("Ground Detection")]
    [Tooltip("Desde dónde nace el rayo, relativo al pivot del jugador. " +
             "Debe estar a la ALTURA DE LOS PIES o apenas por encima — nunca sobre la cabeza.")]
    [SerializeField] private float rayOriginOffsetY = -1.2f;

    [Tooltip("Cuánto puede subir Squishy por encima de los pies del jugador antes de " +
             "ignorar el suelo detectado. Evita que trepe a plataformas que el player no alcanzó.")]
    [SerializeField] private float maxRiseAbovePlayer = 0.5f;

    [Tooltip("Profundidad de pozo que dispara el 'salto de fe' mágico.")]
    [SerializeField] private float pitDepthThreshold = 2f;

    private float _currentVelocityX;
    private float _currentVelocityY;

    // FIX #13: el raycast se ejecutaba en Update (frecuencia visual).
    // Separamos: el raycast corre en FixedUpdate (frecuencia física) y guarda el resultado.
    // El SmoothDamp consume ese resultado en Update para movimiento visual suave.
    private float _targetY;
    private bool _hasGroundTarget;

    private void Start()
    {
        _targetY = transform.position.y; // valor inicial seguro
    }

    // FIX #13: Raycast en FixedUpdate — respeta el timestep de física
    private void FixedUpdate()
    {

        if (playerTransform == null) return;   // 📖 sin esto, un prefab sin asignar tira NRE cada tick de física

        float facing = Mathf.Sign(playerTransform.localScale.x);
        float targetX = playerTransform.position.x + xOffset * facing;

        // 📖 EL FIX: el rayo nace a la altura de los PIES
        float playerFeetY = playerTransform.position.y + rayOriginOffsetY;
        Vector2 rayOrigin = new Vector2(targetX, playerFeetY);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * raycastDistance, hit.collider != null ? Color.green : Color.red);


        if (hit.collider != null)
        {
            // 📖 Sin suelo detectado, Squishy acompaña al jugador en vez de congelarse.
            // El código anterior dejaba _hasGroundTarget en false y la Y quedaba clavada
            // para siempre — el slime se colgaba en el aire.
            _targetY = playerTransform.position.y - playerFeetOffset;
            _hasGroundTarget = true;
            return;
        }

        float groundY = hit.point.y + yOffset;
        float depthBelowPlayer = playerFeetY - groundY;

        // 📖 Doble guarda, una por cada lado:
        //   depthBelowPlayer >  pitDepthThreshold  → pozo profundo → "salto de fe" (📌 GDD)
        //   depthBelowPlayer < -maxRiseAbovePlayer → suelo por ENCIMA → lo ignoramos
        bool pitTooDeep = depthBelowPlayer > pitDepthThreshold;
        bool groundTooHigh = depthBelowPlayer < -maxRiseAbovePlayer;

        _targetY = (pitTooDeep || groundTooHigh)
            ? playerTransform.position.y - playerFeetOffset
            : groundY;

        _hasGroundTarget = true;
    }

    // SmoothDamp permanece en Update para movimiento visual suave sin tartamudeo
    private void Update()
    {

        float facing = Mathf.Sign(playerTransform.localScale.x);

        float newX = Mathf.SmoothDamp(
            transform.position.x,
            playerTransform.position.x + xOffset * facing,
            ref _currentVelocityX,
            smoothTimeX
        );

        float newY = _hasGroundTarget
            ? Mathf.SmoothDamp(transform.position.y, _targetY, ref _currentVelocityY, smoothTimeY)
            : transform.position.y; // sin suelo detectado, mantiene Y actual

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}