using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Configuración de la Bala")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask damageLayers; // 📌 GDD: capas que pueden recibir daño
    [SerializeField] private float lifetime = 3f;

    [Header("Effects")]
    // 📖 La dependencia de audio ahora está ACÁ, visible en el Inspector del
    // prefab, en vez de escondida dentro de un AudioManager.Instance.
    [Tooltip("Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxChannel;
    [Tooltip("La receta del sonido de impacto (Cue, no AudioClip).")]
    [SerializeField] private AudioCue impactCue;


    [Header("VFX")]
    [Tooltip("Arrastrá VFXCueChannel.asset.")]
    [SerializeField] private VFXCueChannel vfxChannel;
    [Tooltip("Qué tipo de explosión pedir al impactar.")]
    [SerializeField] private VFXType impactVFX;

    // FIX #1: era IBulletPool (interfaz custom incompatible con lo que Initialize recibía)
    // Ahora es IObjectPool<Bullet> para matchear el parámetro de Initialize
    private IObjectPool<Bullet> _pool;
    private Rigidbody2D _rb;
    private float _lifetimeTimer;
    private bool _isReleased;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // FIX #5: el timer de vida existía declarado pero nunca se decrementaba
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
            ReturnToPool();
    }

    /// <summary>
    /// Inyección de dependencias: llamada por el pool al crear la bala (solo una vez en toda su vida).
    /// </summary>
    // FIX #1: parámetro ahora es IObjectPool<Bullet>, que es lo que BulletPool pasa realmente
    public void Initialize(IObjectPool<Bullet> pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// Llamado por BulletPool.actionOnGet cada vez que la bala es extraída del pool.
    /// Resetea el estado interno para que funcione correctamente en su próximo turno.
    /// </summary>
    // FIX #7: _isReleased nunca se reseteaba → bala era inmune en su segunda vida
    public void ResetState()
    {
        _isReleased = false;
        _lifetimeTimer = lifetime; // FIX #5: timer reseteado al salir del pool
    }

    /// <summary>
    /// Aplica velocidad y orienta el sprite. Llamado por BulletPool.Get() después de posicionar.
    /// </summary>
    // FIX #3: era "Disparar" — nombre en español mezclado con código en inglés
    public void Fire(Vector2 direction)
    {
        _rb.velocity = direction * speed;
        // Flip visual del sprite según dirección horizontal
        transform.localScale = new Vector3(Mathf.Sign(direction.x), 1f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isReleased) return;

        // FIX #6: antes no se chequeaba damageLayers ni se llamaba a IDamageable
        bool isInDamageLayer = (damageLayers.value & (1 << collision.gameObject.layer)) != 0;

        if (isInDamageLayer && collision.TryGetComponent(out IDamageable damageable))
        {
            // 🔵 NET: en multijugador, TakeDamage debe ejecutarse solo en el servidor
            damageable.TakeDamage(damage, _rb.velocity.normalized);
        }

        SpawnEffects();
        ReturnToPool();
    }

    private void SpawnEffects()
    {
        if(vfxChannel != null)
            vfxChannel.RaiseEvent(impactVFX, transform.position);

        // 📖 Chequeamos != null y NO usamos "sfxChannel?.RaiseEvent(...)".
        // El operador ?. de C# solo ve el null real; el == de Unity está
        // sobrecargado y también detecta objetos destruidos o referencias rotas.
        // En Unity, SIEMPRE el chequeo explícito.
        // 📖 No hace falta chequear impactCue: RaiseEvent lo transporta tal cual
        // y el AudioManager descarta los cues null o inválidos.
        if (sfxChannel != null)
            sfxChannel.RaiseEvent(impactCue, transform.position);
    }

    private void ReturnToPool()
    {
        if (_isReleased) return;
        _isReleased = true;
        _rb.velocity = Vector2.zero;
        _pool?.Release(this);
    }
}
