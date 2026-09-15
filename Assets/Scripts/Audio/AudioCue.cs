using UnityEngine;
using UnityEngine.Audio;


    /// <summary>
    /// Receta de sonido reutilizable. Agrupa TODAS las variantes de un mismo evento
    /// sonoro (ej: los 4 wavs de "paso sobre tierra") junto con las reglas de cómo
    /// debe sonar: rango de volumen, rango de pitch y a qué bus del mixer va.
    /// </summary>
    // 🧩 Patrón: "Data as Asset" — la configuración vive en un archivo, no en código.
    [CreateAssetMenu(fileName = "Cue_", menuName = "Audio/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        [Header("Clips — variantes del MISMO sonido")]
        [SerializeField] private AudioClip[] clips;

        [Header("Randomización — mata el efecto ametralladora")]
        // 📖 Vector2 usado como "rango": x = mínimo, y = máximo.
        // No es una posición, es un par de floats. Unity lo dibuja lindo en el Inspector.
        [SerializeField] private Vector2 volumeRange = new Vector2(0.85f, 1.0f);
        [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);

        [Header("Ruteo y espacialización")]
        [SerializeField] private AudioMixerGroup mixerGroup;

        // 📌 GDD: el juego es 2D. spatialBlend 0 = sonido plano (se escucha igual esté
        // donde esté). 1 = sonido 3D posicional. Dejalo en 0 salvo que quieras que un
        // enemigo lejano suene más bajo.
        [Range(0f, 1f)]
        [SerializeField] private float spatialBlend = 0f;

        // 📖 [System.NonSerialized]: este índice NO se guarda dentro del .asset en disco.
        // Sin esto, cada vez que jugaras, Unity marcaría el asset como "modificado" y
        // te ensuciaría el git status con cambios que no hiciste.
        [System.NonSerialized] private int _lastIndex = -1;

        public bool IsValid => clips != null && clips.Length > 0;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public float SpatialBlend => spatialBlend;

        /// <summary>Volumen tope del cue. Lo usa la música (no queremos música con volumen random).</summary>
        public float MaxVolume => Mathf.Max(volumeRange.x, volumeRange.y);

        /// <summary>
        /// Devuelve un clip al azar, garantizando que NO sea el mismo que la vez anterior.
        /// </summary>
        public AudioClip NextClip()
        {
            if (!IsValid) return null;
            if (clips.Length == 1) return clips[0];

            int index = Random.Range(0, clips.Length);

            // 📖 Si salió el mismo de la vez pasada, lo corremos uno.
            // El módulo (%) hace que si estabas en el último, volvés al 0. Nunca sale fuera de rango.
            if (index == _lastIndex) index = (index + 1) % clips.Length;

            _lastIndex = index;
            return clips[index];
        }

        public float RandomVolume() => Random.Range(volumeRange.x, volumeRange.y);
        public float RandomPitch() => Random.Range(pitchRange.x, pitchRange.y);

        /// <summary>
        /// Configura el AudioSource y dispara un one-shot.
        /// </summary>
        /// <returns>Duración real en segundos (ajustada por pitch), o 0 si no sonó nada.</returns>
        public float PlayOneShotOn(AudioSource source)
        {
            if (source == null) return 0f;

            AudioClip clip = NextClip();
            if (clip == null) return 0f;

            ApplyRoutingTo(source);
            source.pitch = RandomPitch();

            // 📖 PlayOneShot vs Play: PlayOneShot NO pisa source.clip y permite que varios
            // sonidos se superpongan en el MISMO AudioSource. Para pasos es exactamente
            // lo que querés (un paso puede empezar antes de que termine el anterior).
            source.PlayOneShot(clip, RandomVolume());

            // 📖 Un pitch de 1.5 hace que el clip suene 1.5x más rápido → dura menos.
            // Si devolviéramos clip.length crudo, el pool retendría el objeto de más.
            return clip.length / Mathf.Max(0.01f, source.pitch);
        }

        /// <summary>Aplica mixer group y espacialización, sin reproducir nada.</summary>
        public void ApplyRoutingTo(AudioSource source)
        {
            if (source == null) return;
            if (mixerGroup != null) source.outputAudioMixerGroup = mixerGroup;
            source.spatialBlend = spatialBlend;
        }

#if UNITY_EDITOR
        // 📖 OnValidate corre cada vez que tocás un valor en el Inspector.
        // Lo usamos para que no puedas configurar rangos imposibles (pitch 0 = silencio eterno).
        private void OnValidate()
        {
            pitchRange.x = Mathf.Max(0.05f, pitchRange.x);
            pitchRange.y = Mathf.Max(pitchRange.x, pitchRange.y);
            volumeRange.x = Mathf.Clamp01(volumeRange.x);
            volumeRange.y = Mathf.Clamp(volumeRange.y, volumeRange.x, 1f);
        }
#endif
    }


