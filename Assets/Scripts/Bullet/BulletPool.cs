using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool reutilizable de balas. Implementa IBulletPool para ser consumido por
/// PlayerCombat y EnemyShooter sin que conozcan los detalles del pool interno.
/// </summary>
public class BulletPool : MonoBehaviour, IBulletPool
{
    [Header("Configuration")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 50;

    private IObjectPool<Bullet> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Bullet>(
            createFunc: CreateBullet,
            actionOnGet: OnTakeFromPool,      // FIX #2: este método existía referenciado pero nunca definido
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: Application.isEditor, // 🟡 PERF: doble-release check solo en editor
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    /// <summary>
    /// Punto de entrada público. Posiciona, activa y dispara la bala en un solo paso.
    /// </summary>
    public Bullet Get(Vector2 position, Vector2 direction)
    {
        Bullet bullet = _pool.Get();          // dispara OnTakeFromPool primero
        bullet.transform.position = position;
        bullet.gameObject.SetActive(true);
        bullet.Fire(direction);               // FIX #3: era bullet.Fire() pero Bullet tenía "Disparar" → no compilaba
        return bullet;
    }

    public void Release(Bullet bullet) => _pool.Release(bullet);

    // ── Reglas del pool ───────────────────────────────────────────────────────

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.Initialize(_pool);             // inyección: le damos el "boleto de regreso"
        return bullet;
    }

    // FIX #2: método estaba referenciado en el constructor del ObjectPool pero nunca definido → no compilaba
    // FIX #7: llamamos ResetState() para que _isReleased y el lifetime timer queden limpios
    private void OnTakeFromPool(Bullet bullet)
    {
        bullet.ResetState();
    }

    private void OnReturnToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet bullet)
    {
        if (bullet != null)
            Destroy(bullet.gameObject);
    }
}
