using UnityEngine;

/// <summary>
/// AudioSource reciclable. NO decide cuándo volver al pool — de eso se encarga
/// el AudioManager. Este script solo sabe reproducir y reportar cuánto dura.
/// </summary>
// ⚠️ SOLID (SRP): antes esta clase reproducía Y gestionaba su propio retorno al
// pool con una corrutina. Dos responsabilidades. Ahora solo reproduce.
[RequireComponent(typeof(AudioSource))]
public class PooledAudio : MonoBehaviour
{
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();

        // 📖 FIX del defecto #2: aunque destildes Play On Awake en el prefab,
        // esto lo fuerza por código. Sin esto, cada vez que el pool hace
        // SetActive(true) el AudioSource reproduce el clip ANTERIOR que quedó
        // cacheado, antes de que le asignemos el nuevo. Se oye como un click.
        _source.playOnAwake = false;
    }

    /// <summary>Reproduce un cue. Devuelve la duración real en segundos.</summary>
    public float Play(AudioCue cue)
    {
        if (cue == null) return 0f;
        _source.loop = false;
        return cue.PlayOneShotOn(_source);
    }

    /// <summary>Corta el sonido de inmediato. Lo llama el pool al reciclar.</summary>
    public void StopAndReset()
    {
        _source.Stop();
        _source.clip = null;
        _source.pitch = 1f;
    }
}