using System.Collections;
using UnityEngine;
using UnityEngine.Audio;


    /// <summary>
    /// Reproductor de música persistente con crossfade entre tracks.
    /// Sobrevive los cambios de escena y responde a un MusicCueChannel.
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicManager : MonoBehaviour
    {
        [Header("Canal de eventos")]
        [SerializeField] private MusicCueChannel musicChannel;

        [Header("Ruteo")]
        [SerializeField] private AudioMixerGroup musicGroup;

        [Header("Fade")]
        [SerializeField] private float defaultFadeDuration = 1.5f;

        // 🧩 Patrón: Double Buffering. Dos AudioSources en vez de uno.
        // Analogía en Paso 3.
        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _activeSource;

        private AudioCue _currentCue;
        private Coroutine _fadeRoutine;

        private static MusicManager _instance;

        private void Awake()
        {
            // 📖 Mismo guard clause que tu GameManager. El enabled = false ANTES del
            // Destroy es clave: evita que Unity llame a OnEnable en el duplicado y
            // lo suscriba al canal justo antes de morir (suscripción zombie).
            if (_instance != null && _instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }
            _instance = this;

            transform.parent = null;          // DontDestroyOnLoad solo funciona en objetos raíz
            DontDestroyOnLoad(gameObject);

            _sourceA = CreateMusicSource("Music_A");
            _sourceB = CreateMusicSource("Music_B");
            _activeSource = _sourceA;
        }

        private AudioSource CreateMusicSource(string sourceName)
        {
            // 📖 Creamos los AudioSource por código en vez de pedírtelos en el Inspector:
            // menos setup manual, imposible de configurar mal.
            GameObject child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;   // 📌 GDD: música siempre 2D, nunca posicional
            source.volume = 0f;
            source.outputAudioMixerGroup = musicGroup;
            return source;
        }

        private void OnEnable()
        {
            if (musicChannel != null) musicChannel.OnRaiseEvent += OnMusicRequested;
        }

        private void OnDisable()
        {
            // 📖 ESTÁNDAR NO NEGOCIABLE: todo += tiene su -= gemelo.
            // Sin esto, el canal (que es un asset, no se destruye) mantiene viva
            // la referencia a este objeto para siempre → memory leak.
            if (musicChannel != null) musicChannel.OnRaiseEvent -= OnMusicRequested;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnMusicRequested(AudioCue cue, float fadeDuration)
        {
            PlayMusic(cue, fadeDuration > 0f ? fadeDuration : defaultFadeDuration);
        }

        /// <summary>Cambia el track actual con crossfade. cue == null funde a silencio.</summary>
        public void PlayMusic(AudioCue cue, float fadeDuration)
        {
            // 📖 Guard clause clave: si ya estás sonando ESE track, no hacés nada.
            // Sin esto, al volver del Game Over al menú la música se reiniciaría
            // desde el segundo 0 en vez de seguir sonando. Detalle chico, se nota mucho.
            if (cue == _currentCue) return;

            _currentCue = cue;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeRoutine(cue, fadeDuration));
        }

        public void StopMusic(float fadeDuration) => PlayMusic(null, fadeDuration);

        private IEnumerator CrossfadeRoutine(AudioCue cue, float duration)
        {
            AudioSource from = _activeSource;
            AudioSource to = (_activeSource == _sourceA) ? _sourceB : _sourceA;

            float targetVolume = 0f;

            if (cue != null && cue.IsValid)
            {
                to.clip = cue.NextClip();
                to.pitch = 1f;               // la música nunca lleva pitch random
                to.volume = 0f;
                to.loop = true;
                cue.ApplyRoutingTo(to);
                to.spatialBlend = 0f;        // fuerza 2D aunque el cue diga otra cosa
                to.Play();
                targetVolume = cue.MaxVolume;
            }

            float fromStartVolume = from.volume;
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                // 📖 unscaledDeltaTime en vez de deltaTime: si pausás el juego con
                // Time.timeScale = 0, deltaTime queda en 0 y el fade se congela para
                // siempre. El audio NO se ve afectado por timeScale, así que el fade
                // tampoco debería.
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                from.volume = Mathf.Lerp(fromStartVolume, 0f, t);
                to.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            from.volume = 0f;
            from.Stop();
            from.clip = null;               // libera la referencia al AudioClip

            to.volume = targetVolume;
            _activeSource = to;
            _fadeRoutine = null;
        }
    }

