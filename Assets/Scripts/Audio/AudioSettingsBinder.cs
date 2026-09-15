using UnityEngine;
using UnityEngine.UI;


    /// <summary>
    /// Aplica los volúmenes guardados al arrancar, y conecta sliders si los hay.
    /// Ponelo en el objeto persistente de audio (sin sliders) y en el menú (con sliders).
    /// </summary>
    public class AudioSettingsBinder : MonoBehaviour
    {
        [SerializeField] private AudioSettingsSO settings;

        [Header("Sliders (opcionales — dejalos vacíos fuera del menú)")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        // 📖 Start y NO Awake. AudioMixer.SetFloat es famoso por fallar silenciosamente
        // si lo llamás en Awake: el mixer todavía no terminó de inicializarse y el
        // valor se descarta sin error. Es una de las trampas clásicas de Unity.
        private void Start()
        {
            if (settings == null)
            {
                Debug.LogError("[AudioSettingsBinder] Falta asignar el AudioSettingsSO.", this);
                return;
            }

            settings.LoadAndApply();

            Bind(masterSlider, settings.Master, settings.SetMaster);
            Bind(musicSlider, settings.Music, settings.SetMusic);
            Bind(sfxSlider, settings.Sfx, settings.SetSfx);
        }

        private void Bind(Slider slider, float initialValue, UnityEngine.Events.UnityAction<float> callback)
        {
            if (slider == null) return;

            // 📖 SetValueWithoutNotify: pone el valor visual SIN disparar onValueChanged.
            // Si usaras slider.value = x, se dispararía el callback y reescribirías
            // PlayerPrefs innecesariamente en cada arranque.
            slider.SetValueWithoutNotify(initialValue);
            slider.onValueChanged.AddListener(callback);
        }

        private void OnDestroy()
        {
            // ESTÁNDAR: todo AddListener tiene su RemoveListener.
            if (masterSlider != null) masterSlider.onValueChanged.RemoveAllListeners();
            if (musicSlider != null) musicSlider.onValueChanged.RemoveAllListeners();
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveAllListeners();
        }
    }
