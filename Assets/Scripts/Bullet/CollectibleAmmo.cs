using UnityEngine;

/// <summary>
/// Munición que recoge el Brujo al tocarla: suma balas una sola vez y pide su
/// destello y su sonido por canal, sin conocer al VFXManager ni al AudioManager.
/// </summary>
public class CollectibleAmmo : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 1; // cuánta munición otorga este coleccionable

    [Header("Efectos al recoger")]
    [Tooltip("Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxChannel;
    [Tooltip("La receta del sonido de recogida (Cue, no AudioClip).")]
    [SerializeField] private AudioCue pickupCue;
    [Tooltip("Arrastrá VFXCueChannel.asset.")]
    [SerializeField] private VFXCueChannel vfxChannel;
    [SerializeField] private VFXType pickupVFX = VFXType.AmmoPickup;

    private bool _collected;

    public int AmmoAmount => ammoAmount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    /// <summary>
    /// Regla de recogida: solo el Brujo, y una sola vez. Devuelve si la recogió.
    /// </summary>
    // Los eventos de trigger 2D llegan aunque el componente esté desactivado, y nada
    // garantiza uno solo por paso de física: _collected corta cualquier segunda llamada
    // antes de sumar munición o repetir el destello y el sonido.
    public bool TryCollect(GameObject toucher)
    {
        if (_collected || !toucher.CompareTag("Player")) return false;
        if (!toucher.TryGetComponent(out PlayerBlackboard blackboard)) return false;

        _collected = true;
        blackboard.bulletCount += ammoAmount;
        SpawnEffects();
        gameObject.SetActive(false);
        return true;
    }

    private void SpawnEffects()
    {
        // Chequeo explícito != null (el == de Unity detecta referencias rotas); el
        // AudioManager descarta un cue null, así que pickupCue no hace falta chequearlo
        if (vfxChannel != null)
            vfxChannel.RaiseEvent(pickupVFX, transform.position);

        if (sfxChannel != null)
            sfxChannel.RaiseEvent(pickupCue, transform.position);
    }
}
