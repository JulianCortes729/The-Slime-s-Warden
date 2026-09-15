using UnityEngine;
using UnityEngine.Audio;


    /// <summary>
    /// Punto único de control de volúmenes. Es un asset, así que cualquier escena
    /// puede referenciarlo desde el Inspector sin buscar nada en runtime.
    /// </summary>
    // 🧩 Patrón: Service as ScriptableObject — reemplaza al Singleton sin usar
    // FindObjectOfType ni estáticos.
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio/Audio Settings")]
    public class AudioSettingsSO : ScriptableObject
    {
        [SerializeField] private AudioMixer mixer;

        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";

        private const string MasterKey = "vol_master";
        private const string MusicKey = "vol_music";
        private const string SfxKey = "vol_sfx";

        public float Master { get; private set; } = 1f;
        public float Music { get; private set; } = 1f;
        public float Sfx { get; private set; } = 1f;

        /// <summary>Lee PlayerPrefs y aplica todo al mixer. Llamalo desde un Start().</summary>
        public void LoadAndApply()
        {
            Master = PlayerPrefs.GetFloat(MasterKey, 1f);
            Music = PlayerPrefs.GetFloat(MusicKey, 1f);
            Sfx = PlayerPrefs.GetFloat(SfxKey, 1f);

            Apply(MasterParam, Master);
            Apply(MusicParam, Music);
            Apply(SfxParam, Sfx);
        }

        public void SetMaster(float linear01) => Set(MasterParam, MasterKey, linear01, v => Master = v);
        public void SetMusic(float linear01) => Set(MusicParam, MusicKey, linear01, v => Music = v);
        public void SetSfx(float linear01) => Set(SfxParam, SfxKey, linear01, v => Sfx = v);

        private void Set(string param, string key, float linear01, System.Action<float> store)
        {
            linear01 = Mathf.Clamp01(linear01);
            store(linear01);
            PlayerPrefs.SetFloat(key, linear01);
            Apply(param, linear01);
        }

        private void Apply(string param, float linear01)
        {
            if (mixer == null) return;
            mixer.SetFloat(param, LinearToDecibels(linear01));
        }

        /// <summary>
        /// Convierte 0..1 del slider a decibeles del mixer.
        /// </summary>
        // 📖 El oído humano percibe el volumen de forma LOGARÍTMICA, no lineal.
        // Si mandaras el 0.5 del slider directo como -0.5 dB, no notarías diferencia.
        // La fórmula log10(v) * 20 hace que "medio slider" suene realmente a la mitad.
        // -80 dB es el silencio total del mixer de Unity.
        private static float LinearToDecibels(float linear01)
        {
            return linear01 <= 0.0001f ? -80f : Mathf.Log10(linear01) * 20f;
        }
    }
