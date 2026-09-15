using UnityEngine;

/// <summary>
/// Gestiona el disparo del jugador. Lee el Blackboard y delega el disparo
/// al BulletPool inyectado — no conoce los detalles del pool interno.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerBlackboard blackboard;

    // FIX #8: antes PlayerCombat creaba su propio ObjectPool<Bullet> duplicando
    // exactamente lo que BulletPool.cs ya hace. Ahora inyectamos la abstracción.
    // ⚠️ SOLID: dependencia hacia la interfaz, no hacia la implementación concreta.
    [SerializeField] private BulletPool bulletPool; // expuesto como BulletPool para el inspector; usado como IBulletPool

    [SerializeField] private Transform firePoint;

    [Header("Audio")]
    [Tooltip("Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxChannel;
    [Tooltip("La receta del sonido de disparo (Cue, no AudioClip).")]
    [SerializeField] private AudioCue shootCue;

    private IBulletPool _bulletPool; // ⚠️ SOLID: referencia interna tipada como interfaz

    private void Awake()
    {
        _bulletPool = bulletPool; // cast implícito: BulletPool implementa IBulletPool
    }

    private void Update()
    {
        if (blackboard.isTalking) return; // No podemos disparar si estamos hablando o muertos
        if (blackboard.isDead) return;
        // FIX #15: era blackboard.contBullet → renombrado a bulletCount
        if (blackboard.shootIntent && blackboard.bulletCount > 0)
            Shoot();
    }

    private void Shoot()
    {
        blackboard.shootIntent = false;
        blackboard.bulletCount--;

        // 📖 Antes: AudioManager.Instance.PlayAudioClip(...). Esta clase tenía que
        // saber que existía un AudioManager, que era singleton y que ya estaba vivo.
        // Ahora solo grita en un canal. Si nadie escucha, no pasa nada.
        if (sfxChannel != null)
            sfxChannel.RaiseEvent(shootCue, firePoint.position);

        // Dirección determinada por el flip visual del jugador
        float dirX = Mathf.Sign(transform.localScale.x);
        _bulletPool.Get(firePoint.position, new Vector2(dirX, 0f));
    }
}

