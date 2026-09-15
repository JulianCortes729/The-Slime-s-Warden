using System;
using UnityEngine;


/// <summary>
/// Canal de eventos para pedir la reproducción de un sonido puntual.
/// Desacopla a los emisores (PlayerCombat, EnemyShooter, Bullet) del AudioManager:
/// ninguno de los dos lados conoce al otro, solo comparten este asset.
/// </summary>
[CreateAssetMenu(fileName = "AudioCueChannel", menuName = "Event Channels/Audio Cue Channel")]
public class AudioCueChannel : ScriptableObject
{
    public event Action<AudioCue, Vector3> OnRaiseEvent;

    public void RaiseEvent(AudioCue cue, Vector3 position)
    {
        OnRaiseEvent?.Invoke(cue, position);
    }



    // 📖 Los ScriptableObject son assets: sobreviven al Stop del Editor cuando
    // "Reload Domain" está desactivado. Sin este limpiado, en la segunda sesión
    // de Play el evento todavía apunta al AudioManager de la sesión anterior
    // (ya destruido) → MissingReferenceException fantasma, intermitente.
    private void OnDisable()
    {
        OnRaiseEvent = null;
    }
}

