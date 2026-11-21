using UnityEngine;

/// <summary>
/// Gestiona el disparo del jugador. Lee el Blackboard y delega el disparo
/// al BulletPool inyectado — no conoce los detalles del pool interno.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerBlackboard blackboard;

    // El pool de balas compartido de la escena: PlayerCombat no conoce su interior
    [SerializeField] private BulletPool bulletPool;

    [SerializeField] private Transform firePoint;

    [Header("Audio")]
    [Tooltip("Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxChannel;
    [Tooltip("La receta del sonido de disparo (Cue, no AudioClip).")]
    [SerializeField] private AudioCue shootCue;

    private void Update()
    {
        if (blackboard.IsControlLocked)
        {
            // No podemos disparar si estamos hablando o muertos, y un disparo pulsado justo antes se descarta
            blackboard.shootIntent = false;
            return;
        }
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
        bulletPool.Get(firePoint.position, new Vector2(dirX, 0f));
    }
}

