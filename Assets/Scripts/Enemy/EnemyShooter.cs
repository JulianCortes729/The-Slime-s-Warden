using UnityEngine;

/// <summary>
/// Enemigo que detecta al jugador dentro de un radio y dispara en línea de visión.
/// Usa NonAlloc para la detección y delega el pool de balas al BulletPool inyectado.
/// </summary>
/// // 📌 GDD: "Slimes Francotiradores — enemigos estacionarios que actúan como torretas"
public class EnemyShooter : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float radius;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform firePoint;

    [Header("Disparo")]
    [SerializeField] private float fireCooldown = 1f;

    [Header("Audio")]
    [Tooltip("Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxChannel;
    [Tooltip("La receta del sonido de disparo (Cue, no AudioClip).")]
    [SerializeField] private AudioCue shootCue;

    // FIX #8/#9: antes EnemyShooter duplicaba todo el ObjectPool<Bullet> que ya existe en BulletPool.cs
    // Ahora inyectamos la abstracción — EnemyShooter no sabe cómo funciona el pool internamente.
    [SerializeField] private BulletPool bulletPool;   // inspector: tipo concreto para asignar en editor
    private IBulletPool _bulletPool;                  // uso interno: tipado como interfaz (SOLID / DIP)

    private float _currentCooldown;

    // FIX #12: usamos buffer NonAlloc para la detección; un solo resultado es suficiente
    // 🟢 POOL: evitamos allocations en el hot-path de detección
    private readonly Collider2D[] _detectionBuffer = new Collider2D[1];

    private void Awake()
    {
        _bulletPool = bulletPool;
    }

    private void Update()
    {
        if (_currentCooldown > 0f)
            _currentCooldown -= Time.deltaTime;
    }

    // FIX #12: la detección física pasó de Update a FixedUpdate para respetar el timestep de física
    private void FixedUpdate()
    {
        if (_currentCooldown > 0f) return;

        // 🟡 PERF: OverlapCircleNonAlloc — sin allocations, escribe en el buffer pre-reservado
        int hits = Physics2D.OverlapCircleNonAlloc(firePoint.position, radius, _detectionBuffer, targetLayer);
        if (hits == 0) return;

        Collider2D target = _detectionBuffer[0];
        RotateTurret(target.transform.position);

        Vector2 directionToTarget = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;
        float distanceToTarget = Vector2.Distance(firePoint.position, target.transform.position);

        // Verificación de línea de visión: ¿hay un obstáculo entre el cañón y el jugador?
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToTarget, distanceToTarget, obstacleLayer);
        Debug.DrawRay(firePoint.position, directionToTarget * distanceToTarget, Color.yellow);

        if (hit.collider == null)
        {
            Shoot(directionToTarget);
            _currentCooldown = fireCooldown;
        }
    }

    private void Shoot(Vector2 direction)
    {
        // 📖 Mismo cambio que en PlayerCombat: gritamos en el canal en vez de
        // buscar un singleton. Fijate que ahora Player y Enemy disparan sonido
        // de forma IDÉNTICA, sin compartir una clase base ni conocerse.
        if (sfxChannel != null)
            sfxChannel.RaiseEvent(shootCue, firePoint.position);

        // FIX #20: antes Get() devolvía la bala pero la dirección se perdía porque
        // OnTakeBulletFromPool no la tenía — ahora IBulletPool.Get() lo maneja todo en un paso.
        _bulletPool.Get(firePoint.position, direction);
    }

    private void RotateTurret(Vector3 targetPosition)
    {
        bool targetIsLeft = targetPosition.x < transform.position.x;
        transform.rotation = targetIsLeft
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.identity;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (firePoint == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        Gizmos.DrawSphere(firePoint.position, 0.05f);
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawWireSphere(firePoint.position, radius);
    }
}

