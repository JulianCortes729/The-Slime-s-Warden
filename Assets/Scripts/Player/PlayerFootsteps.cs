using UnityEngine;
/// <summary>
/// Dispara sonidos de paso según la distancia recorrida por el jugador.
/// No toca el pool del AudioManager: usa su propio AudioSource dedicado.
/// </summary>
// 📌 GDD: "el movimiento busca ser ágil y preciso" — el audio debe acompañar
// la velocidad real, no un timer fijo.
[RequireComponent(typeof(PlayerBlackboard))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerBlackboard blackboard;
    [SerializeField] private Rigidbody2D rb;

    [Tooltip("AudioSource dedicado. Si lo dejás vacío, se busca en este GameObject.")]
    [SerializeField] private AudioSource footstepSource;

    [Header("Cues")]
    [Tooltip("Superficie por defecto (opción 3.1). El hook para 3.2 ya está listo.")]
    [SerializeField] private AudioCue defaultSurfaceCue;

    [Tooltip("Opcional. Dejalo vacío si no querés sonido de aterrizaje.")]
    [SerializeField] private AudioCue landingCue;

    [Header("Ritmo")]
    [Tooltip("Unidades de mundo recorridas entre paso y paso.")]
    [SerializeField] private float stepDistance = 1.2f;

    [Tooltip("Input mínimo para considerar que el jugador está caminando.")]
    [SerializeField] private float minMoveInput = 0.1f;

    [Tooltip("Anti-spam: tiempo mínimo entre dos pasos, pase lo que pase.")]
    [SerializeField] private float minStepInterval = 0.12f;

    [Tooltip("Velocidad de caída mínima para que el aterrizaje suene.")]
    [SerializeField] private float minLandingFallSpeed = 3f;

    private float _travelled;
    private float _lastStepTime = -99f;
    private bool _wasGrounded;
    private float _verticalSpeedLastFrame;

    private void Awake()
    {
        // 📖 ESTÁNDAR: todas las referencias se resuelven en Awake, UNA vez.
        // Nunca GetComponent en Update — es una búsqueda lineal por los componentes
        // del GameObject cada frame.
        if (blackboard == null) blackboard = GetComponent<PlayerBlackboard>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();

        if (footstepSource == null)
        {
            Debug.LogError("[PlayerFootsteps] Falta un AudioSource. Agregale uno al Player.", this);
            enabled = false;
            return;
        }

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f;   // 📌 GDD: 2D
    }

    // 📖 FixedUpdate y no Update: leemos rb.velocity, que solo se actualiza en el
    // paso de física. En Update leerías el mismo valor 2 o 3 veces seguidas y la
    // distancia acumulada quedaría mal en monitores de 144Hz.
    private void FixedUpdate()
    {
        bool grounded = blackboard.isGrounded;

        DetectLanding(grounded);

        _wasGrounded = grounded;
        _verticalSpeedLastFrame = rb.velocity.y;

        if (!CanStep(grounded))
        {
            // 📖 Al frenar dejamos el acumulador "cargado". Así el primer paso
            // al retomar la marcha suena de inmediato en vez de tardar 1.2 unidades.
            // El minStepInterval evita que esto se convierta en spam si zapateás A-D-A-D.
            _travelled = stepDistance;
            return;
        }

        // 📖 velocity.x en unidades/segundo × segundos del tick = unidades recorridas.
        // Abs porque caminar a la izquierda también cuenta como distancia.
        _travelled += Mathf.Abs(rb.velocity.x) * Time.fixedDeltaTime;

        if (_travelled < stepDistance) return;

        // 📖 RESTAMOS en vez de asignar 0. Si recorriste 1.35 y el paso es cada 1.2,
        // el sobrante de 0.15 se conserva para el próximo. Sin esto, a velocidades
        // altas se te desfasa el ritmo progresivamente.
        _travelled -= stepDistance;

        PlayCue(ResolveSurfaceCue());
    }

    private void DetectLanding(bool grounded)
    {
        // Flanco de subida: estaba en el aire y ahora toca piso.
        if (!grounded || _wasGrounded) return;

        if (landingCue != null && _verticalSpeedLastFrame <= -minLandingFallSpeed)
            PlayCue(landingCue);

        // El siguiente paso sale rápido después de aterrizar.
        _travelled = stepDistance * 0.5f;
    }

    private bool CanStep(bool grounded)
    {
        // 📌 GDD: sin pasos en el aire, ni muerto, ni mientras volás por un knockback.
        if (!grounded) return false;
        if (blackboard.isDead) return false;
        if (blackboard.isKnockedBack) return false;
        if (Mathf.Abs(blackboard.moveInput) < minMoveInput) return false;
        return true;
    }

    /// <summary>
    /// 🔌 HOOK para la opción 3.2 (detección de superficie).
    /// Hoy devuelve siempre el mismo cue. Cuando quieras madera en las plataformas
    /// frágiles y tierra en el tilemap, el único cambio va acá adentro: un
    /// Physics2D.Raycast hacia abajo + TryGetComponent&lt;SurfaceTag&gt;().
    /// Ni PlayerFootsteps ni nada más se entera del cambio.
    /// </summary>
    private AudioCue ResolveSurfaceCue()
    {
        return defaultSurfaceCue;
    }

    private void PlayCue(AudioCue cue)
    {
        if (cue == null || !cue.IsValid) return;

        float now = Time.unscaledTime;
        if (now - _lastStepTime < minStepInterval) return;
        _lastStepTime = now;

        cue.PlayOneShotOn(footstepSource);
    }
}
