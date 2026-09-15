using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

/// <summary>
/// Servicio de SFX persistente. Mantiene un pool de AudioSources y los devuelve
/// automáticamente cuando el sonido terminó — sin corrutinas y sin generar basura.
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    // 📖 static PRIVADO, no público. Su único trabajo es que el segundo
    // AudioManager que aparezca se dé cuenta y se destruya. Nadie de afuera
    // puede llegar acá → se acabaron las dependencias ocultas.
    // 📖 Ojo con el == de Unity: está sobrecargado. Si _instance quedó apuntando
    // a un objeto destruido (típico con "Reload Domain" desactivado), este
    // chequeo devuelve "es null" igual y el nuevo AudioManager toma el mando.
    private static AudioManager _instance;

    [Header("Channels")]
    [Tooltip("El único camino para pedir un SFX. Arrastrá SFXCueChannel.asset.")]
    [SerializeField] private AudioCueChannel sfxCueChannel;


    [Header("Pool")]
    [SerializeField] private PooledAudio audioPrefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 24;

    private IObjectPool<PooledAudio> _pool;

    // 📖 struct, no class: son datos puros sin identidad propia. Viven en el
    // array interno de la List, no en el heap → cero presión sobre el GC.
    private struct ActiveSound
    {
        public PooledAudio Source;
        public float ReleaseTime;
    }

    // 🟢 POOL: la List se dimensiona una vez en el constructor y después
    // nunca vuelve a reservar memoria (mientras no pases de maxSize).
    private readonly List<ActiveSound> _activeSounds = new List<ActiveSound>(32);

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // 📖 enabled = false ANTES del Destroy: impide que Unity le llame
            // OnEnable al duplicado y lo suscriba al canal justo antes de morir.
            // Sin esto tendrías un suscriptor zombie dentro del ScriptableObject.
            enabled = false;
            Destroy(gameObject);
            return;
        }
        _instance = this;

        transform.parent = null; // DontDestroyOnLoad solo funciona en objetos raíz
        DontDestroyOnLoad(gameObject);

        _pool = new ObjectPool<PooledAudio>(
            createFunc: CreatePooledAudio,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPooled,
            collectionCheck: Application.isEditor,  // 🟡 PERF: chequeo de doble-release solo en editor
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private void OnEnable()
    {
        if (_instance != this) return;

        if (sfxCueChannel == null)
        {
            // 📖 LogError con "this" como segundo parámetro: al clickear el error
            // en la consola, Unity te resalta ESTE GameObject en la jerarquía.
            Debug.LogError("[AudioManager] Falta asignar el AudioCueChannel. No va a sonar ningún SFX.", this);
            return;
        }

        sfxCueChannel.OnRaiseEvent += OnCueRaised;
    }

    // 📖 todo += tiene su -= gemelo. El canal es un asset
    // (no se destruye nunca), así que sin esto mantendría viva la referencia a
    // este objeto para siempre → memory leak.
    private void OnDisable()
    {
        if (_instance != this) return;

        if (sfxCueChannel != null)
            sfxCueChannel.OnRaiseEvent -= OnCueRaised;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    private void OnCueRaised(AudioCue cue, Vector3 position)
    {
        PlayCue(cue, position);
    }

    // ── Reglas del pool ───────────────────────────────────────────────────────

    private PooledAudio CreatePooledAudio()
    {
        // 📖 FIX del hallazgo #7 — LA LÍNEA MÁS IMPORTANTE DE TODO ESTE ARCHIVO.
        // El segundo parámetro (transform) hace que la instancia nazca como hija
        // del AudioManager. Como el AudioManager tiene DontDestroyOnLoad, los
        // hijos lo heredan y sobreviven el cambio de escena.
        // Sin esto: al cargar SampleScene, Unity destruye estos objetos pero el
        // pool sigue guardando las referencias → MissingReferenceException.
        return Instantiate(audioPrefab, transform);
    }

    private void OnTakeFromPool(PooledAudio pooled)
    {
        pooled.gameObject.SetActive(true);
    }

    private void OnReturnToPool(PooledAudio pooled)
    {
        pooled.StopAndReset();
        pooled.gameObject.SetActive(false);
    }

    private void OnDestroyPooled(PooledAudio pooled)
    {
        if (pooled != null) Destroy(pooled.gameObject);
    }

    // ── Ciclo de vida de los sonidos activos ──────────────────────────────────

    private void Update()
    {
        // 📖 unscaledTime: el audio sigue sonando aunque pauses con timeScale = 0.
        // Si usáramos Time.time, en pausa los sonidos nunca volverían al pool
        // y en 20 sonidos el pool se agotaría.
        float now = Time.unscaledTime;

        // 📖 Recorremos AL REVÉS. Si borrás mientras vas hacia adelante, los
        // índices se corren y te salteás elementos. Yendo hacia atrás, borrar
        // el índice i no afecta a los que todavía no visitaste.
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            if (now < _activeSounds[i].ReleaseTime) continue;

            PooledAudio finished = _activeSounds[i].Source;
            _activeSounds.RemoveAt(i);   // 🟢 POOL: RemoveAt sobre List<struct> no aloca
            _pool.Release(finished);
        }
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Reproduce un AudioCue (con variación de clip, volumen y pitch).</summary>
    ///   // 📖 PRIVADO a propósito. El canal es la única puerta de entrada.
    // Si mañana querés disparar un sonido desde un UnityEvent del Inspector,
    // la solución NO es hacer esto público: es un componente chiquito que
    // levante el canal. Volver a abrir esta puerta es volver al Service Locator.
    private void PlayCue(AudioCue cue, Vector3 position)
    {
        if (cue == null || !cue.IsValid) return;

        PooledAudio pooled = _pool.Get();
        pooled.transform.position = position;

        float duration = pooled.Play(cue);
        Track(pooled, duration);
    }

    private void Track(PooledAudio pooled, float duration)
    {
        if (duration <= 0f)
        {
            // No sonó nada → devolución inmediata, no lo trackeamos.
            _pool.Release(pooled);
            return;
        }

        _activeSounds.Add(new ActiveSound
        {
            Source = pooled,
            // +0.05s de colchón para que el último sample no se corte.
            ReleaseTime = Time.unscaledTime + duration + 0.05f
        });
    }
}